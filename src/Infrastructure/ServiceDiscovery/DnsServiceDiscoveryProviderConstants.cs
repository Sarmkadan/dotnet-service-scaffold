#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Constants for DnsServiceDiscoveryProvider to avoid magic values.
/// </summary>
internal static class DnsServiceDiscoveryProviderConstants
{
    // String literals for logging and error messages
    public const string ServiceHasNoDnsRecords = "Service {ServiceName} has no DNS records";
    public const string DnsResolutionFailed = "DNS resolution failed for service {ServiceName}";
    public const string DnsProviderDoesNotSupportProgrammaticRegistration = "DNS provider does not support programmatic registration.";
    public const string DnsProviderDoesNotSupportProgrammaticDeregistration = "DNS provider does not support programmatic deregistration.";
    public const string DnsReadOnly = "DNS_READ_ONLY";
    public const string FailedToResolveServiceDuringWatch = "Failed to resolve service {ServiceName} during watch: {ErrorMessage}. Retrying in {RefreshInterval}...";
    public const string SrvQueryTimedOut = "SRV query for {Fqdn} timed out on attempt {Attempt}/{Max}";

    // DNS protocol constants
    public const int DnsHeaderLength = 12;
    public const int DnsTransactionIdOffset = 0;
    public const int DnsTransactionIdLength = 2;
    public const int DnsFlagsHighOffset = 2;
    public const int DnsFlagsLowOffset = 3;
    public const int DnsRcodeMask = 0x0F;
    public const int DnsAnswerCountOffset = 6;
    public const int DnsAnswerCountLength = 2;
    public const int DnsQuestionOffsetAfterName = 4; // QTYPE (2) + QCLASS (2)
    public const int DnsResourceRecordFixedPartLength = 10; // TYPE(2) + CLASS(2) + TTL(4) + RDLENGTH(2)
    public const int DnsSrvRecordMinRdataLength = 6; // PRIORITY(2) + WEIGHT(2) + PORT(2)
    public const byte DnsPointerFlag = 0xC0;
    public const ushort DnsPointerOffsetMask = 0x3F;
    public const byte DnsNullTerminator = 0;
    public const int DnsLengthFieldSize = 1; // Size of the length field in DNS labels
}