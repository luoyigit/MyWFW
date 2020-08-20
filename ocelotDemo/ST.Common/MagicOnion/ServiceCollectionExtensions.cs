using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ST.Common.Consul.Options;
using ST.Common.MagicOnion.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ST.Common.MagicOnion
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGrpcClient(this IServiceCollection services)
        {
            services.AddSingleton<IGRpcConnection, GRpcConnection>();
            services.AddSingleton<IGrpcChannelFactory, GrpcChannelFactory>();
            return services;
        }
       
        public static IServiceCollection AddGrpc(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<RpcDiscoveryOptions>(configurationSection);

            return services;
        }


        public static IApplicationBuilder UseRpc(this IApplicationBuilder app)
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
            var serviceId = $"{rpcOptions.Value.ServiceName}_{addresses.First().Host}:{rpcOptions.Value.Port}";
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                #region 注册rpc
                //设置启动服务的ID
                if (rpcOptions != null)
                {
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
                        Address = addresses.First().Host,               //启动服务的地址
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
                if (rpcOptions != null)
                {
                    consulClient.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
                }
                Bootstrap.Stop();
            });

            //启动rpc服务
            Bootstrap.InitalizeRpcSetting(rpcOptions.Value.Host, rpcOptions.Value.Port);
            Bootstrap.Start();
            return app;
        }
    }
}
