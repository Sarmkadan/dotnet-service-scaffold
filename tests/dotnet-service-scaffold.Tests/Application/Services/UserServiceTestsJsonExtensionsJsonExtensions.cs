using System;
using System.Text.Json;
using System.Collections.Generic;

namespace dotnet_service_scaffold.Tests.Application.Services
{
    public static class UserServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamePolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this UserServiceTestsJsonExtensions value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static UserServiceTestsJsonExtensions? FromJson(string json)
        {
            return JsonSerializer.Deserialize<UserServiceTestsJsonExtensions>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out UserServiceTestsJsonExtensions? value)
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