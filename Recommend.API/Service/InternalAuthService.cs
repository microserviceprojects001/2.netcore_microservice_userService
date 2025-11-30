// Contact.API/Services/ClientAuthTestService.cs
using Microsoft.Extensions.Options;

using Consul;
using Resilience;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Recommend.API.Dtos;

namespace Recommend.API.Service
{
    public interface IInternalAuthService
    {
        Task<string> GetServiceTokenAsync();
    }

    public class InternalAuthService : IInternalAuthService
    {
        private readonly IHttpClient _httpClient;
        private readonly IConsulClient _consulClient;
        private readonly ServerDiscoveryConfig _options;
        private readonly ClientSettings _clientSettings;
        private readonly ILogger<InternalAuthService> _logger;

        public InternalAuthService(
            IHttpClient httpClient,
            IConsulClient consulClient,
            IOptions<ServerDiscoveryConfig> options,
            IOptions<ClientSettings> clientSettings,
            ILogger<InternalAuthService> logger)
        {
            _httpClient = httpClient;
            _consulClient = consulClient;
            _options = options.Value;
            _logger = logger;
            _clientSettings = clientSettings.Value;
        }


        public async Task<string> GetServiceTokenAsync()
        {
            try
            {
                // 从Consul获取IdentityServer服务地址
                var services = await _consulClient.Health.Service(_options.IdentityServiceName, tag: null, passingOnly: true);
                var service = services.Response.FirstOrDefault();

                if (service == null)
                {
                    throw new Exception("No healthy instances of IdentityServer found");
                }

                // 构建IdentityServer地址
                var identityServerUrl = new UriBuilder
                {
                    Scheme = service.Service.Tags.Contains("https") ? "https" : "http",
                    Host = service.Service.Address,
                    Port = service.Service.Port
                }.ToString();

                // 手动创建令牌请求
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientSettings.ClientId),
                    new KeyValuePair<string, string>("client_secret", _clientSettings.ClientSecret),
                    new KeyValuePair<string, string>("scope", "contact_api.internal")
                });

                // 创建临时HttpClient用于获取令牌
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(identityServerUrl);

                var response = await httpClient.PostAsync("/connect/token", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Token request failed: {response.StatusCode}. Error: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get service token");
                throw;
            }
        }

        // 用于反序列化令牌响应的辅助类
        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }
        }
    }
}