using System.Net;

namespace Recommend.API.Dtos;

/// <summary>
/// t推荐服务只需要 用户的基本信息，所有只需要配置用户的 服务发现信息， 不需要发现服务注册自己到consul
/// </summary>
public class ServerDiscoveryConfig
{
    public bool UseHttps { get; set; }
    public string UserServiceName { get; set; }
    public string ContactServiceName { get; set; }
    public string IdentityServiceName { get; set; }
    public ConsulConfig Consul { get; set; }
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