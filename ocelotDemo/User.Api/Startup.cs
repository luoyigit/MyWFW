using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using ST.Common.Consul;
using User.Api.Data;
using User.Api.Filters;
using User.Api.IntergrationEventService;
using User.Api.Models;

namespace User.Api
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
            //添加服务发现  进行配置绑定             
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();
            //services.AddControllers();
            services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            services.AddDbContext<UserContext>(builder =>
            {
                builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("userApi", new OpenApiInfo() { Title = "User API 接口", Version = "v1" });
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "User.Api.xml");
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
            services.AddTransient<IUserCreateSubscriberService, UserCreateSubscriberService>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    //options.Authority = "http://localhost:9000";
                    options.Authority = "http://localhost:61114"; //一般写网关地址进行转发（因为indentity server 可能有多个）
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "user_api";

                });
            services.AddCap(options =>
            {
                //docker安装RabbitMQ：docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3-management
                options.UseEntityFramework<UserContext>()
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
                    d.DiscoveryServerHostName = "192.168.1.165";
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 5001;
                    d.NodeId = "1";
                    d.NodeName = "CAP User API Node";
                });
            });
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
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.ShowExtensions();
                options.SwaggerEndpoint("/swagger/userApi/swagger.json", "UserApi V1");
            });
            app.UseCapDashboard();
            //app.UseCap();
            //启用服务注册于发现
            app.UseConsul();
            //dataInit(app);
        }


        private void dataInit(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserContext>();
            dbContext.Database.Migrate();
            if (!dbContext.Users.Any())
            {
                dbContext.Users.Add(new AppUser()
                {
                    Name = "luoyi",
                    Title = "产品经理",
                    Phone = "123456"
                });
                dbContext.SaveChanges();
            }
        }
    }
}
