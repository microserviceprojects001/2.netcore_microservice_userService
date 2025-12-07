using System.Net.Http;
using System;
using System.Threading.Tasks;
using Consul;
using User.Identity.Dtos;
using Microsoft.Extensions.Options;
using Resilience;
using Newtonsoft.Json;

namespace User.Identity.Services;

public class UserService : IUserService
{
    private readonly IHttpClient _httpClient;
    private readonly IConsulClient _consulClient;
    private readonly ServerDiscoveryConfig _options;
    //private readonly string _userServiceUrl = "https://localhost:5201";
    private readonly ILogger<UserService> _logger;
    public UserService(
          IHttpClient httpClient,
          IConsulClient consulClient,
          IOptions<ServerDiscoveryConfig> options,
          ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _consulClient = consulClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UserInfo> CheckOrCreate(string phone)
    {
        _logger.LogInformation("[UserService] 开始CheckOrCreate，手机号: {Phone}", phone);

        // 检查当前跟踪上下文
        var activity = System.Diagnostics.Activity.Current;
        _logger.LogInformation("[UserService] 当前TraceId: {TraceId}",
            activity?.TraceId.ToString() ?? "无跟踪上下文");

        // 从Consul获取服务地址
        var services = await _consulClient.Health.Service(_options.UserServiceName, tag: null, passingOnly: true);
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
            Path = "/api/users/check-or-create"
        }.ToString();

        var form = new Dictionary<string, string>
            {
                { "phone", phone }
            };
        try
        {
            _logger.LogInformation("[UserService] 调用User.API: {Uri}", uri);

            var response = await _httpClient.PostAsync(uri, form);

            _logger.LogInformation("[UserService] 响应状态码: {StatusCode}", response.StatusCode);

            // 检查响应头中是否有跟踪信息
            foreach (var header in response.Headers)
            {
                if (header.Key.StartsWith("trace") || header.Key.StartsWith("X-B3"))
                {
                    _logger.LogDebug("[UserService] 响应跟踪头: {Key}={Value}",
                        header.Key, string.Join(",", header.Value));
                }
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(result);
                _logger.LogInformation($"Completed check-or-create with userID: {userInfo.Id}");
                return userInfo;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling check-or-create");
            throw ex;
        }
        return null;
    }
}