using System.Net;

namespace ST.Common.Consul.Options
{
    /// <summary>
    /// Dns 目的地址
    /// </summary>
    public class DnsEndpoint
    {
        /// <summary>
        /// IP 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        public IPEndPoint ToIPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(Address), Port);
        }
    }
}