using Consul;
using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ST.Common.Consul.Options;
using ST.Common.MagicOnion;
using ST.Common.MagicOnion.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ST.Common.Consul
{
    public static class ConsulExtensions
    {
        /// <summary>
        /// 添加 ConsulClient 服务依赖，需添加配置ServiceDiscoveryOptions
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsulClient(this IServiceCollection services,
            IConfigurationSection configurationSection)
        {
            services.Configure<ServiceDiscoveryOptions>(configurationSection);
            services.AddSingleton<IConsulClient>(provider => new ConsulClient(cfg =>
            {
                //读取 Consul 的配置信息
                var serviceConfiguration = provider.GetRequiredService<IOptions<ServiceDiscoveryOptions>>()?.Value;
                if (!string.IsNullOrWhiteSpace(serviceConfiguration.Consul.HttpEndpoint))
                {
                    //如果不配置，ConsulClient将采用默认地址：127.0.0.1:8500
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));
            //添加rpc
            services.AddGrpcClient();
            return services;
        }

        public static IServiceCollection AddConsulClient(this IServiceCollection services,
            Action<ServiceDiscoveryOptions> optionAction)
        {
            //添加配置文件注入
            if (optionAction == null)
                throw new ArgumentNullException(nameof(optionAction));

            services.Configure(optionAction);
            services.AddSingleton<IConsulClient>(provider => new ConsulClient(cfg =>
            {
                //读取 Consul 的配置信息
                var serviceConfiguration = provider.GetRequiredService<IOptions<ServiceDiscoveryOptions>>()?.Value;
                if (!string.IsNullOrWhiteSpace(serviceConfiguration.Consul.HttpEndpoint))
                {
                    //如果不配置，ConsulClient将采用默认地址：127.0.0.1:8500
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));
            return services;
        }


        /// <summary>
        /// 添加DnsClient 服务依赖，需加配置ServiceDiscoveryOptions
        /// </summary>
        /// <param name="services"></param>
        public static void AddDnsClient(this IServiceCollection services)
        {
            services.AddSingleton<IDnsQuery>(b =>
            {
                var serviceOption = b.GetRequiredService<IOptions<ServiceDiscoveryOptions>>();
                if (serviceOption.Value == null)
                {
                    return new LookupClient(IPAddress.Parse("127.0.0.1"), 8600);
                }
                //添加Consul服务地址
                return new LookupClient(serviceOption.Value.Consul.DnsEndpoint.ToIPEndPoint());
            });
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var applicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            if (applicationLifetime == null)
                throw new ArgumentNullException(nameof(applicationLifetime));
            var consulClient = app.ApplicationServices.GetService<IConsulClient>();

            if (consulClient == null)
                throw new ArgumentNullException(nameof(consulClient));

            var serviceOptions = app.ApplicationServices.GetRequiredService<IOptions<ServiceDiscoveryOptions>>();
            if (serviceOptions == null)
                throw new ArgumentNullException(nameof(serviceOptions));

            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            if (env == null)
                throw new ArgumentNullException(nameof(env));

            //获取服务启动地址绑定信息
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(o => new Uri(o));

            //在服务启动时,向Consul 中心进行注册
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                foreach (var address in addresses)
                {
                    //设置启动服务的ID
                    var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                    //设置健康检查
                    var httpCheck = new AgentServiceCheck()
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1), //错误时间超过1分钟，移除
                        Interval = TimeSpan.FromSeconds(10), //30秒检查一次
                        HTTP = new Uri(address, "HealthCheck").OriginalString
                    };

                    var registration = new AgentServiceRegistration()
                    {
                        Checks = new[] { httpCheck },            //配置健康检查
                        Address = address.Host,                //启动服务的地址
                        Port = address.Port,                   //启动服务的端口
                        ID = serviceId,                        //服务唯一ID
                        Name = serviceOptions.Value.ServiceName//对外服务名称
                    };
                    //向Consul 中心进行注册
                    consulClient.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
                }


                //register localhost address
                //注册本地地址
                //var localhostregistration = new AgentServiceRegistration()
                //{
                //    Checks = new[] { new AgentServiceCheck()
                //{
                //    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                //    Interval = TimeSpan.FromSeconds(10),
                //    //HTTP = $"{Uri.UriSchemeHttp}://localhost:{addresses.First().Port}/HealthCheck",
                //    HTTP =new Uri(addresses.First(), "HealthCheck").OriginalString
                //} },
                //    Address = "localhost",
                //    ID = $"{serviceOptions.Value.ServiceName}_localhost:{addresses.First().Port}",
                //    Name = serviceOptions.Value.ServiceName,
                //    Port = addresses.First().Port
                //};

                //consulClient.Agent.ServiceRegister(localhostregistration).GetAwaiter().GetResult();
            });

            //在程序停止时,向Consul 中心进行注销
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                foreach (var address in addresses)
                {
                    //设定服务Id(全局唯一 unique）
                    var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";
                    consulClient.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
                }
            });

            return app;
        }


        public static IApplicationBuilder UseConsulWithRpc(this IApplicationBuilder app)
        {
            var applicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            if (applicationLifetime == null)
                throw new ArgumentNullException(nameof(applicationLifetime));
            var consulClient = app.ApplicationServices.GetService<IConsulClient>();

            if (consulClient == null)
                throw new ArgumentNullException(nameof(consulClient));

            var serviceOptions = app.ApplicationServices.GetRequiredService<IOptions<ServiceDiscoveryOptions>>();
            if (serviceOptions == null)
                throw new ArgumentNullException(nameof(serviceOptions));

            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            if (env == null)
                throw new ArgumentNullException(nameof(env));

            //获取服务启动地址绑定信息
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(o => new Uri(o));

            var rpcOptions = app.ApplicationServices.GetRequiredService<IOptions<RpcDiscoveryOptions>>();
            //在服务启动时,向Consul 中心进行注册
            applicationLifetime.ApplicationStarted.Register(() =>
            {
               
                foreach (var address in addresses)
                {
                    //设置启动服务的ID
                    var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                    //设置健康检查
                    var httpCheck = new AgentServiceCheck()
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1), //错误时间超过1分钟，移除
                        Interval = TimeSpan.FromSeconds(10), //30秒检查一次
                        HTTP = new Uri(address, "HealthCheck").OriginalString
                    };

                    var registration = new AgentServiceRegistration()
                    {
                        Checks = new[] { httpCheck },            //配置健康检查
                        Address = address.Host,                //启动服务的地址
                        Port = address.Port,                   //启动服务的端口
                        ID = serviceId,                        //服务唯一ID
                        Name = serviceOptions.Value.ServiceName//对外服务名称
                    };
                    //向Consul 中心进行注册
                    consulClient.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
                }

                #region 注册rpc
                //设置启动服务的ID
                if(rpcOptions != null)
                {
                    var serviceId = $"{rpcOptions.Value.ServiceName}_{rpcOptions.Value.Host}:{rpcOptions.Value.Port}";

                    //设置健康检查
                    var httpCheck = new AgentServiceCheck()
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1), //错误时间超过1分钟，移除
                        Interval = TimeSpan.FromSeconds(10), //30秒检查一次
                        HTTP = new Uri(addresses.First(), "HealthCheck").OriginalString
                    };

                    var registration = new AgentServiceRegistration()
                    {
                        Checks = new[] { httpCheck },            //配置健康检查
                        Address = rpcOptions.Value.Host,                //启动服务的地址
                        Port = rpcOptions.Value.Port,                   //启动服务的端口
                        ID = serviceId,                        //服务唯一ID
                        Name = rpcOptions.Value.ServiceName//对外服务名称
                    };
                    //向Consul 中心进行注册
                    consulClient.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
                }
               
                #endregion
            });

            //在程序停止时,向Consul 中心进行注销
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                foreach (var address in addresses)
                {
                    //设定服务Id(全局唯一 unique）
                    var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";
                    consulClient.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
                }

                if(rpcOptions != null)
                {
                    var serviceId = $"{rpcOptions.Value.ServiceName}_{rpcOptions.Value.Host}:{rpcOptions.Value.Port}";
                    consulClient.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();

                    
                }
                

            });

            return app;
        }
    }
}
