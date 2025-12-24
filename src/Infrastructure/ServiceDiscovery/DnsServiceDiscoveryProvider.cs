// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Resolves service instances using DNS SRV records with automatic A-record fallback.
/// Sends raw UDP DNS queries to the configured nameserver so that SRV record types —
/// not exposed by <see cref="System.Net.Dns"/> — can be interrogated directly.
/// </summary>
public sealed class DnsServiceDiscoveryProvider : IServiceDiscoveryProvider
{
    private const ushort DnsTypeA = 1;
    private const ushort DnsTypeSrv = 33;

    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<DnsServiceDiscoveryProvider> _logger;
    private int _transactionSeed = Random.Shared.Next(1, ushort.MaxValue);

    /// <inheritdoc/>
    public string ProviderName => "DNS";

    /// <summary>
    /// Initialises a new <see cref="DnsServiceDiscoveryProvider"/> with the supplied options.
    /// </summary>
    public DnsServiceDiscoveryProvider(
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<DnsServiceDiscoveryProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dns = _options.Dns;
            List<ServiceDiscoveryRecord> records = [];

            if (dns.PreferSrvRecords)
            {
                // Kubernetes DNS-SD pattern: _<service>._tcp.<namespace>.svc.cluster.local
                var srvFqdn = $"_{serviceName.ToLowerInvariant()}._tcp.{dns.SearchDomain}";
                var srvRecords = await QuerySrvAsync(srvFqdn, cancellationToken);

                foreach (var srv in srvRecords)
                {
                    try
                    {
                        var addresses = await System.Net.Dns.GetHostAddressesAsync(srv.Target, cancellationToken);
                        foreach (var addr in addresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork))
                        {
                            records.Add(BuildRecord(serviceName, addr.ToString(), srv.Port,
                                dns.DefaultScheme, srv.Weight, srv.Priority, srv.Ttl));
                        }
                    }
                    catch (SocketException ex)
                    {
                        _logger.LogDebug("Could not resolve A record for SRV target {Target}: {Error}", srv.Target, ex.Message);
                    }
                }
            }

            if (records.Count == 0)
            {
                // Fallback: plain A-record lookup for <service>.<searchdomain>
                var aFqdn = $"{serviceName.ToLowerInvariant()}.{dns.SearchDomain}";
                _logger.LogDebug("SRV lookup returned no results; falling back to A record for {Fqdn}", aFqdn);

                var addresses = await System.Net.Dns.GetHostAddressesAsync(aFqdn, cancellationToken);
                foreach (var addr in addresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork))
                {
                    records.Add(BuildRecord(serviceName, addr.ToString(), dns.DefaultPort, dns.DefaultScheme));
                }
            }

            _logger.LogDebug("DNS resolved {Count} instance(s) for {ServiceName}", records.Count, serviceName);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(records);
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostNotFound)
        {
            _logger.LogDebug("Service {ServiceName} has no DNS records", serviceName);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(Array.Empty<ServiceDiscoveryRecord>());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DNS resolution failed for service {ServiceName}", serviceName);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure(ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>DNS is a read-only backend; programmatic registration is not supported.</remarks>
    public Task<Result> RegisterAsync(ServiceDiscoveryRecord record, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure("DNS provider does not support programmatic registration.", "DNS_READ_ONLY"));

    /// <inheritdoc/>
    public Task<Result> DeregisterAsync(Guid instanceId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure("DNS provider does not support programmatic deregistration.", "DNS_READ_ONLY"));

    /// <inheritdoc/>
    /// <remarks>
    /// Polls <see cref="ResolveAsync"/> at <see cref="ServiceDiscoveryOptions.RefreshInterval"/>
    /// and yields a new snapshot whenever the set of endpoint URIs changes.
    /// </remarks>
    public async IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync(
        string serviceName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var previousUris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await ResolveAsync(serviceName, cancellationToken);
            if (result.IsSuccess && result.Value is { } current)
            {
                var currentUris = current.Select(r => r.ToEndpointUri())
                                         .ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (!currentUris.SetEquals(previousUris))
                {
                    previousUris = currentUris;
                    yield return current;
                }
            }

            try { await Task.Delay(_options.RefreshInterval, cancellationToken); }
            catch (OperationCanceledException) { yield break; }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dns = _options.Dns;
            var ep = new IPEndPoint(IPAddress.Parse(dns.DnsServerAddress), dns.DnsServerPort);
            var txId = (ushort)(Interlocked.Increment(ref _transactionSeed) & 0xFFFF);
            var query = BuildDnsQuery(".", DnsTypeA, txId);

            using var udp = new UdpClient();
            udp.Client.ReceiveTimeout = (int)dns.SocketTimeout.TotalMilliseconds;
            await udp.SendAsync(query, query.Length, ep);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<SrvRecord>> QuerySrvAsync(string fqdn, CancellationToken cancellationToken)
    {
        var dns = _options.Dns;
        var ep = new IPEndPoint(IPAddress.Parse(dns.DnsServerAddress), dns.DnsServerPort);
        var txId = (ushort)(Interlocked.Increment(ref _transactionSeed) & 0xFFFF);
        var query = BuildDnsQuery(fqdn, DnsTypeSrv, txId);

        for (int attempt = 0; attempt <= dns.MaxRetries; attempt++)
        {
            using var udp = new UdpClient();
            udp.Client.ReceiveTimeout = (int)dns.SocketTimeout.TotalMilliseconds;

            try
            {
                await udp.SendAsync(query, query.Length, ep);
                var received = await udp.ReceiveAsync(cancellationToken);
                return ParseSrvResponse(received.Buffer, txId);
            }
            catch (SocketException) when (attempt < dns.MaxRetries)
            {
                _logger.LogDebug("SRV query for {Fqdn} timed out on attempt {Attempt}/{Max}", fqdn, attempt + 1, dns.MaxRetries + 1);
            }
        }

        return [];
    }

    private static byte[] BuildDnsQuery(string fqdn, ushort qtype, ushort transactionId)
    {
        var buf = new List<byte>(64);

        // Header — all multi-byte fields in network (big-endian) byte order
        buf.Add((byte)(transactionId >> 8));
        buf.Add((byte)(transactionId & 0xFF));
        buf.Add(0x01); // Flags high: RD=1
        buf.Add(0x00); // Flags low
        buf.Add(0x00); buf.Add(0x01); // QDCOUNT = 1
        buf.Add(0x00); buf.Add(0x00); // ANCOUNT = 0
        buf.Add(0x00); buf.Add(0x00); // NSCOUNT = 0
        buf.Add(0x00); buf.Add(0x00); // ARCOUNT = 0

        // QNAME: length-prefixed labels terminated by a zero octet
        foreach (var label in fqdn.TrimEnd('.').Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var labelBytes = Encoding.ASCII.GetBytes(label);
            buf.Add((byte)labelBytes.Length);
            buf.AddRange(labelBytes);
        }
        buf.Add(0x00);

        // QTYPE
        buf.Add((byte)(qtype >> 8));
        buf.Add((byte)(qtype & 0xFF));

        // QCLASS = IN (1)
        buf.Add(0x00);
        buf.Add(0x01);

        return [.. buf];
    }

    private static List<SrvRecord> ParseSrvResponse(byte[] data, ushort expectedTxId)
    {
        if (data.Length < 12) return [];

        var txId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(0, 2));
        if (txId != expectedTxId) return [];

        bool isResponse = (data[2] & 0x80) != 0;
        int rcode = data[3] & 0x0F;
        if (!isResponse || rcode != 0) return [];

        int anCount = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(6, 2));
        if (anCount == 0) return [];

        int offset = 12;

        // Skip question section: QNAME + QTYPE(2) + QCLASS(2)
        SkipDomainName(data, ref offset);
        offset += 4;

        var results = new List<SrvRecord>(anCount);

        for (int i = 0; i < anCount && offset + 10 <= data.Length; i++)
        {
            SkipDomainName(data, ref offset);
            if (offset + 10 > data.Length) break;

            ushort rrType = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2));
            int ttl = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset + 4, 4));
            int rdLength = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset + 8, 2));
            offset += 10;

            if (rrType == DnsTypeSrv && rdLength >= 6 && offset + rdLength <= data.Length)
            {
                int priority = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2));
                int weight = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset + 2, 2));
                int port = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset + 4, 2));
                int targetOffset = offset + 6;
                string target = ReadDomainName(data, ref targetOffset);

                results.Add(new SrvRecord(priority, weight, port, target, Math.Abs(ttl)));
            }

            offset += rdLength;
        }

        return results;
    }

    private static void SkipDomainName(byte[] data, ref int offset)
    {
        while (offset < data.Length)
        {
            byte b = data[offset];
            if (b == 0) { offset++; return; }
            if ((b & 0xC0) == 0xC0) { offset += 2; return; } // compression pointer
            offset += 1 + b;
        }
    }

    private static string ReadDomainName(byte[] data, ref int offset)
    {
        var parts = new List<string>(8);
        int current = offset;
        bool jumped = false;

        while (current < data.Length)
        {
            byte b = data[current];

            if (b == 0) { if (!jumped) offset = current + 1; break; }

            if ((b & 0xC0) == 0xC0)
            {
                if (current + 1 >= data.Length) break;
                int ptr = ((b & 0x3F) << 8) | data[current + 1];
                if (!jumped) offset = current + 2;
                jumped = true;
                current = ptr;
                continue;
            }

            int len = b;
            current++;
            if (current + len > data.Length) break;
            parts.Add(Encoding.ASCII.GetString(data, current, len));
            current += len;
        }

        return string.Join('.', parts);
    }

    private static ServiceDiscoveryRecord BuildRecord(
        string serviceName, string host, int port, string scheme,
        int weight = 10, int priority = 0, int? ttl = null) => new()
    {
        ServiceName = serviceName,
        Host = host,
        Port = port,
        Scheme = scheme,
        Weight = Math.Clamp(weight, 1, 100),
        Priority = priority,
        Source = DiscoverySource.Dns,
        DnsTtlSeconds = ttl,
        HealthStatus = DiscoveryHealthStatus.Passing
    };

    private sealed record SrvRecord(int Priority, int Weight, int Port, string Target, int Ttl);
}
