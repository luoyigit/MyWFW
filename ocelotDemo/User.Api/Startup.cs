using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
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
            //��ӷ�����  �������ð�             
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
                options.SwaggerDoc("userApi", new OpenApiInfo() { Title = "User API �ӿ�", Version = "v1" });
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
            //services.AddCap(options =>
            //{
            //    //docker��װRabbitMQ��docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3-management
            //    options.UseEntityFramework<UserContext>()
            //        .UseRabbitMQ("localhost")
            //        .UseDashboard();

            //    options.UseDiscovery(d =>
            //    {
            //        d.DiscoveryServerHostName = "localhost";
            //        d.DiscoveryServerPort = 8500;
            //        d.CurrentNodeHostName = "localhost";
            //        d.CurrentNodePort = 5000;
            //        d.NodeId = "1";
            //        d.NodeName = "CAP User API Node";
            //    });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
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
            //app.UseCapDashboard();
            //���÷���ע���ڷ���
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
                    Title = "��Ʒ����",
                    Phone = "123456"
                });
                dbContext.SaveChanges();
            }
        }
    }
}
