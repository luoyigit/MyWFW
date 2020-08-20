using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Consul;
using DnsClient;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Recommend.API.Data;
using Recommend.API.Dtos;
using Recommend.API.IntergrationEvents.EventHandling;
using Recommend.API.Services;
using Resilience;
using ST.Common.Consul;
using User.Identity.Infrastructure;

namespace Recommend.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            //添加服务发现  进行配置绑定             
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();

            services.AddDbContext<RecommendContext>(builder =>
            {
                builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            //#region Consul and  Consul Service Disvovery
            //services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));

            //services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            //{
            //    var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

            //    if (!string.IsNullOrEmpty(serviceConfiguration.Consul.HttpEndpoint))
            //    {
            //        // if not configured, the client will use the default value "127.0.0.1:8500"
            //        cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
            //    }
            //}));
            //#endregion

            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                return new LookupClient(serviceConfiguration.Consul.DnsEndPoint.ToIpEndPoint());
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
                 //IdentityServer地址
                 options.Authority = Configuration["Gateway:Address"];
                 //对应Idp中ApiResource的Name
                 options.Audience = Configuration["IndetityServer:ApiName"];
                 //不使用https
                 options.RequireHttpsMetadata = false;
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateAudience = false
               };
           });

            #region polly register
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //注入IResilientHttpClientFactory
            services.AddSingleton<IResilientHttpClientFactory, ResilientHttpClientFactory>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilientHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                //设置重试次数
                var retryCount = 6;
                //设置在断路器启用前，允许失败次数
                var exceptionAllowBeforeBreaking = 5;
                return new ResilientHttpClientFactory(logger, httpContextAccessor, retryCount, exceptionAllowBeforeBreaking);
            });

            //注入IHttpClient 用ResilientHttpClient实现
            services.AddSingleton<IHttpClient, ResilientHttpClient>(sp => sp.GetService<IResilientHttpClientFactory>().CreateResilientHttpClient());
            #endregion

            #region 依赖注入
            services.AddSingleton(new HttpClient());
            services.AddScoped(typeof(RecommendContext))
               .AddTransient<IUserService, UserService>()
                .AddTransient<IContactService, ContactService>()
                .AddScoped<ProjectCreatedIntegrationEventHandler>();

            #endregion

            #region CAP
            services.AddCap(options =>
            {
                //docker安装RabbitMQ：docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3-management
                options.UseEntityFramework<RecommendContext>()
                    .UseRabbitMQ(options =>
                    {
                        options.HostName = Configuration["Cap:MqHost"];
                        options.UserName = Configuration["Cap:MqUserName"];
                        options.Password = Configuration["Cap:MqPassword"];

                    })
                    .UseDashboard();

                //服务发现的服务器节点
                options.UseDiscovery(d =>
                {
                    d.DiscoveryServerHostName = Configuration["ServiceDiscovery:Consul:DnsEndpoint:Address"];
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = Configuration["LocalService:HttpHost"]; //; "localhost";
                    d.CurrentNodePort = Convert.ToInt32(Configuration["LocalService:HttpPort"]);
                    //d.NodeId = "2";
                    d.NodeId = Configuration["Cap:ConsulNodeId"];
                    d.NodeName = "CAP RecommendApi Node";
                });
            });
            #endregion

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication(); //获取claims的关键
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseCapDashboard();
            //app.UseCap();
            //启用服务注册于发现
            app.UseConsul();
        }
    }
}
