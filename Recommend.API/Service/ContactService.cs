using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recommend.API.Dtos;
using Microsoft.Extensions.Options;
using Consul;
using Resilience;
using Recommend.API.Dtos;
using Newtonsoft.Json;

namespace Recommend.API.Service
{
    public class ContactService : IContactService
    {
        private readonly IHttpClient _httpClient;
        private readonly IConsulClient _consulClient;
        private readonly ServerDiscoveryConfig _options;

        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly string _userServiceUrl = "https://localhost:5201";
        private readonly ILogger<ContactService> _logger;

        public ContactService(IHttpClient httpClient,
          IConsulClient consulClient,
          IOptions<ServerDiscoveryConfig> options,
          ILogger<ContactService> logger,
          IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _consulClient = consulClient;
            _options = options.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<Contact>> GetUserContactsAsync(int userId)
        {
            // 从Consul获取服务地址
            var services = await _consulClient.Health.Service(_options.ContactServiceName, tag: null, passingOnly: true);
            var service = services.Response.FirstOrDefault();
            if (service == null)
            {
                throw new Exception($"No healthy instances of {_options.UserServiceName} found");
            }

            var uri = new UriBuilder
            {
                Scheme = service.Service.Tags.Contains("https") ? "https" : "http",
                Host = service.Service.Address,
                Port = service.Service.Port,
                Path = $"/api/contacts/{userId}"
            }.ToString();

            try
            {
                // 从 HTTP 上下文中获取 token
                var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                string authorizationToken = null;

                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
                {
                    authorizationToken = authorizationHeader.Substring("Bearer ".Length);
                }
                var response = await _httpClient.GetStringAsync(uri, authorizationToken);
                if (string.IsNullOrEmpty(response))
                {
                    return null;
                }
                var userInfo = JsonConvert.DeserializeObject<List<Contact>>(response);
                _logger.LogInformation($"Completed GetContactsByUserIdAsync with userID: {userId}");
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetContactsByUserIdAsync");
                throw ex;
            }
        }
    }
}