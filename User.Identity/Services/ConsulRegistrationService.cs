using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services;

public class ConsulRegistrationService
{
    private readonly IConsulClient _consulClient;
    private readonly ServerDiscoveryConfig _consulConfig;
    private readonly ILogger<ConsulRegistrationService> _logger;
    private readonly IServer _server;
    private readonly List<string> _registeredIds = new List<string>();

    public ConsulRegistrationService(
        IConsulClient consulClient,
        IOptions<ServerDiscoveryConfig> consulConfig,
        ILogger<ConsulRegistrationService> logger,
        IServer server)
    {
        _consulClient = consulClient;
        _consulConfig = consulConfig.Value;
        _logger = logger;
        _server = server;
    }

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        var serverAddresses = _server.Features.Get<IServerAddressesFeature>();

        if (serverAddresses == null)
        {
            _logger.LogError("无法获取服务器地址特征");
            return;
        }

        if (!serverAddresses.Addresses.Any())
        {
            _logger.LogError("服务器地址集合为空");
            return;
        }

        _logger.LogInformation($"检测到所有服务器地址: {string.Join(", ", serverAddresses.Addresses)}");

        // 获取当前环境的实际配置值
        var useHttps = _consulConfig.UseHttps;
        _logger.LogInformation($"UseHttps配置: {useHttps}");

        // 根据 UseHttps 配置过滤要注册的地址
        var targetScheme = useHttps ? "https" : "http";
        var addressesToRegister = serverAddresses.Addresses
            .Where(addr => addr.StartsWith($"{targetScheme}://"))
            .ToList();

        if (!addressesToRegister.Any())
        {
            _logger.LogWarning($"没有找到 {targetScheme.ToUpper()} 地址可以注册到Consul");
            return;
        }

        _logger.LogInformation($"将注册以下 {targetScheme.ToUpper()} 地址到Consul: {string.Join(", ", addressesToRegister)}");

        foreach (var address in addressesToRegister)
        {
            try
            {
                var uri = new Uri(address);
                var scheme = uri.Scheme; // http 或 https
                var autoDetectedPort = uri.Port;

                // ========== 【核心修改】构建健康检查地址 ==========
                string healthCheckUrl = string.Empty;
                string serviceHost = string.Empty;
                int servicePort = 0;

                // 如果配置中明确指定了健康检查主机和端口，则优先使用（适用于Docker）
                if (!string.IsNullOrEmpty(_consulConfig.HealthCheck?.Host) && _consulConfig.HealthCheck.Port.HasValue)
                {
                    // 使用配置驱动的方式
                    serviceHost = _consulConfig.HealthCheck.Host;
                    servicePort = _consulConfig.HealthCheck.Port.Value;

                    // Docker内部通常用HTTP，但这里使用配置的scheme或默认http
                    var healthCheckScheme = targetScheme; // 简化：容器内部通信使用HTTP
                    healthCheckUrl = $"{healthCheckScheme}://{serviceHost}:{servicePort}{_consulConfig.HealthCheck.Path}";

                    _logger.LogInformation($"✅ 使用配置的健康检查地址: {healthCheckUrl}");
                }

                // ===============================================

                // 服务ID（使用主机和端口确保唯一性）
                var serviceId = $"{_consulConfig.IdentityServiceName}-{serviceHost}:{servicePort}";
                _logger.LogInformation($"✅ 注册服务 Consul serviceId: {serviceId}");
                _registeredIds.Add(serviceId);

                // 构建服务注册信息
                var registration = new AgentServiceRegistration
                {
                    ID = serviceId,
                    Name = _consulConfig.IdentityServiceName,
                    Address = serviceHost,
                    Port = servicePort,
                    Tags = scheme == "https" ? new[] { "https" } : Array.Empty<string>(),
                    Check = new AgentServiceCheck
                    {
                        HTTP = healthCheckUrl,
                        TLSSkipVerify = true, // 开发/Docker环境可跳过证书验证
                        Interval = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(5),
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                    }
                };

                await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
                _logger.LogInformation($"✅ 已注册服务到Consul: {serviceId}");
                _logger.LogInformation($"   协议: {scheme.ToUpper()}");
                _logger.LogInformation($"   地址: {serviceHost}");
                _logger.LogInformation($"   端口: {servicePort}");
                _logger.LogInformation($"   健康检查: {healthCheckUrl}");
                _logger.LogInformation($"   Consul地址: {_consulConfig.Consul?.HttpEndpoint}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 注册服务到Consul失败: {address}");
            }
        }
    }

    public async Task DeregisterAsync(CancellationToken cancellationToken)
    {
        if (!_registeredIds.Any())
        {
            _logger.LogInformation("没有需要注销的服务");
            return;
        }

        _logger.LogInformation($"开始注销 {_registeredIds.Count} 个Consul服务");

        foreach (var serviceId in _registeredIds)
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);
                _logger.LogInformation($"✅ 已注销Consul服务: {serviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 注销Consul服务失败: {serviceId}");
            }
        }

        _registeredIds.Clear();
        _logger.LogInformation("所有服务注销完成");
    }
}