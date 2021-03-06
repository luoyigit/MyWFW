namespace ST.Common.Consul.Options
{
    /// <summary>
    /// 服务发现配置类
    /// </summary>
    public class ServiceDiscoveryOptions
    {
        /// <summary>
        /// 注册的服务名称
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// Consul 节点信息
        /// </summary>
        public ConsulOptions Consul { get; set; }
    }
}