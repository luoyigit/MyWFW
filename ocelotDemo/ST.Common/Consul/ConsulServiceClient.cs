using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Consul;
using ST.Common.Consul.Options;
using System.Linq;
namespace ST.Common.Consul
{
    public class ConsulServiceClient
    {
        public static DnsEndpoint GetServiceInfo(string consulUrl, string serviceName)
        {
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(consulUrl)))
            {
                var services = consulClient.Catalog.Service(serviceName).Result.Response;
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
                    return null;
                }
            }
        }
    }
}
