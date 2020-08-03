using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contact.Api.Data;
using Contact.Api.Dtos;
using Contact.Api.Infrastructure;
using Contact.Api.Services;
using Contact.API.IntergrationEvents.EventHandling;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Resilience;
using ST.Common.Consul;

namespace Contact.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //加载MongoDb配置
            services.Configure<AppSetting>(Configuration);
            ///添加服务发现  进行配置绑定    
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IResilientHttpClientFactory, ResilientHttpClientFactory>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ResilientHttpClient>>();
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                return new ResilientHttpClientFactory(logger, httpContextAccessor, 5, 6);
            });
            services.AddSingleton<IHttpClient, ResilientHttpClient>(provider => provider.GetService<IResilientHttpClientFactory>().CreateResilientHttpClient());

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("contactApi", new OpenApiInfo() { Title = "Contact API 接口", Version = "v1" });
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Contact.Api.xml");
                options.IncludeXmlComments(xmlPath);

                var bearerScheme = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Reference = new OpenApiReference()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                };
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {bearerScheme,new List<string>()}
                });
                options.AddSecurityDefinition("Bearer", bearerScheme);
            });

            //服务注入
            services.AddScoped(typeof(ContactContext));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>();
            services.AddScoped<IContactBookRepository, MongoContactBookRepository>();
            services.AddScoped<UserProfileChangedEventHandler>(); //要注册进来，否则cap找不到订阅者
            services.AddControllers();

            services.AddCap(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServerContact"))
                //.UseRabbitMQ("localhost");
                //.UseRabbitMQ(Configuration["Cap:MqHost"]);
                //.UseRabbitMQ("127.0.0.1");
                .UseRabbitMQ(options =>
                {
                    options.HostName = Configuration["Cap:MqHost"];
                    options.UserName = Configuration["Cap:MqUserName"];
                    options.Password = Configuration["Cap:MqPassword"];

                });

                options.UseDashboard();

                options.UseDiscovery(d =>
                    {
                        d.DiscoveryServerHostName = "192.168.1.165";
                        d.DiscoveryServerPort = 8500;
                        d.CurrentNodeHostName = Configuration["LocalService:HttpHost"]; //; "localhost";
                        d.CurrentNodePort = Convert.ToInt32(Configuration["LocalService:HttpPort"]);
                        //d.NodeId = "2";
                        d.NodeId = Configuration["LocalService:HostTag"];
                        d.NodeName = "CAP ContactAPI Node";
                    });
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["Gateway:Address"];
                    //options.Authority = "http://localhost:61114"; //一般写网关地址进行转发（因为indentity server 可能有多个）
                    options.RequireHttpsMetadata = false;
                    options.ApiName = Configuration["IndetityServer:ApiName"];
                    options.SaveToken = true; //保存token，在发起user_api的时候可以取出带上

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseConsul();
            app.UseAuthentication();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.ShowExtensions();
                options.SwaggerEndpoint("/swagger/contactApi/swagger.json", "ContactApi V1");
            });
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
