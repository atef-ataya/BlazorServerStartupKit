﻿using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Server.Authentication
{
    public static class ServiceExtensions
    {
        public static void AddTokenAuthenticationStateProvider(this IServiceCollection services)
        {
            // Make the same instance accessible as both AuthenticationStateProvider and TokenAuthenticationStateProvider
            services.AddScoped<AuthenticationProvider>();
            services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AuthenticationProvider>());
        }

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
