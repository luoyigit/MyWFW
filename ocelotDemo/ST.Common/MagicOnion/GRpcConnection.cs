using Consul;
using MagicOnion;
using ST.Common.Consul.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MagicOnion.Client;

namespace ST.Common.MagicOnion
{
    public class GRpcConnection : IGRpcConnection
    {
        private IGrpcChannelFactory _grpcChannelFactory;
        private IConsulClient _consulClient;
        private DnsEndpoint consulEndpoint { get; set; }
        public GRpcConnection(IGrpcChannelFactory grpcChannelFactory, IConsulClient consulClient)
        {
            _grpcChannelFactory = grpcChannelFactory;
            _consulClient = consulClient;
        }

        /// <summary>
        /// TODO: 缓存优化
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private DnsEndpoint GetServiceInfo(string serviceName)
        {
            var services = _consulClient.Catalog.Service(serviceName).Result.Response;
            if (services != null && services.Any())
            {
                Random r = new Random((int)DateTime.Now.Ticks);
                int index = r.Next(services.Count());
                var service = services.ElementAt(index);
                return new DnsEndpoint()
                {
                    Address = service.ServiceAddress,
                    Port = service.ServicePort
                };
            }
            else
            {
                throw new Exception("未找到服务器地址");
            }
        }
        public async Task<TService> GetRemoteService<TService>(string serviceName) where TService : IService<TService>
        {
            return await Task.Run(() =>
            {
                var serviceEndpoint = GetServiceInfo(serviceName);
                var serviceChannel = _grpcChannelFactory.Get(serviceEndpoint.Address, serviceEndpoint.Port);
                return MagicOnionClient.Create<TService>(serviceChannel);
            });
        }
    }
}
