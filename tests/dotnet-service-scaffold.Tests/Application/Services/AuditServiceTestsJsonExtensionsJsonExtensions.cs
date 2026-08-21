using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_service_scaffold.Tests.Application.Services
{
    public static class AuditServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamePolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this AuditServiceTestsJsonExtensions value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static AuditServiceTestsJsonExtensions? FromJson(string json)
        {
            return JsonSerializer.Deserialize<AuditServiceTestsJsonExtensions>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out AuditServiceTestsJsonExtensions? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}