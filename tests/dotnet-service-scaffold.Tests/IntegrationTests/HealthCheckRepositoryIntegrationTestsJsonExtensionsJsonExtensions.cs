using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace dotnet_service_scaffold.Tests.IntegrationTests
{
    public static class HealthCheckRepositoryIntegrationTestsJsonExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCase = JsonPropertyNameCase.CamelCase
        };

        public static string ToJson(this HealthCheckRepositoryIntegrationTestsJsonExtensions value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static HealthCheckRepositoryIntegrationTestsJsonExtensions? FromJson(string json)
        {
            return JsonSerializer.Deserialize<HealthCheckRepositoryIntegrationTestsJsonExtensions>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out HealthCheckRepositoryIntegrationTestsJsonExtensions? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}