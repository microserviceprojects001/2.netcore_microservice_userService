using System.Net;

namespace Contact.API.Configuration;

public class ServerDiscoveryConfig
{
    public bool UseHttps { get; set; }

    public string UserServiceName { get; set; }
    public string ContactServiceName { get; set; }
    public string IdentityServiceName { get; set; }
    public ConsulConfig Consul { get; set; }
    public HealthCheckConfig HealthCheck { get; set; } = new HealthCheckConfig();
}
public class HealthCheckConfig
{
    /// <summary>
    /// 健康检查的主机名或IP。
    /// Docker环境应设为服务容器名（如 user-identity），本地开发可留空（自动检测）。
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 健康检查的端口。
    /// Docker环境应设为容器内部端口（如 80），本地开发可留空（自动检测）。
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 健康检查的路径，默认为 /HealthCheck。
    /// </summary>
    public string Path { get; set; } = "/HealthCheck";

    /// <summary>
    /// 是否启用健康检查。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 构建完整的健康检查URL。
    /// </summary>
    public string BuildUrl(string defaultScheme, string defaultHost, int defaultPort)
    {
        var host = string.IsNullOrEmpty(Host) ? defaultHost : Host;
        var port = Port ?? defaultPort;
        var scheme = defaultScheme;

        return $"{scheme}://{host}:{port}{Path}";
    }
}

public class ConsulConfig
{
    public string HttpEndpoint { get; set; }
    public DnsEndpointConfig DnsEndpoint { get; set; }
}

public class DnsEndpointConfig
{
    public string Address { get; set; }
    public int Port { get; set; }

    public IPEndPoint ToIPEndPoint()
    {
        return new IPEndPoint(IPAddress.Parse(Address), Port);
    }
}