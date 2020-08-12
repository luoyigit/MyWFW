using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.API.Applications.Queries;
using Project.API.Applications.Service;
using Project.Domain.AggregatesModel;
using Project.Infrastructure;
using Project.Infrastructure.Repositories;
using ST.Common.Consul;

namespace Project.API
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
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
             .AddDnsClient();

            services.AddDbContext<ProjectContext>(builder =>
            {
                builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
                 //IdentityServer��ַ
                 options.Authority = Configuration["Gateway:Address"];
                 //��ӦIdp��ApiResource��Name
                 options.Audience = Configuration["IndetityServer:ApiName"];
                 //��ʹ��https
                 options.RequireHttpsMetadata = false;
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateAudience = false
               };
           });

            //services.AddDbContext<ProjectContext>(options =>
            //{
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sql =>
            //    {
            //        sql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name); //���dbcontext ��������⣬��Ҫ��������ʵ�������code first
            //    });
            //});

            services.AddScoped<IRecommendService, TestRecommendService>()
                .AddScoped<IProjectQueries, ProjectQueries>(sp => new ProjectQueries(Configuration.GetConnectionString("DefaultConnection"), sp.GetRequiredService<ProjectContext>()))
                .AddScoped<IProjectRepository, ProjectRepository>(sp =>
                {
                    var projectContext = sp.GetRequiredService<ProjectContext>();
                    return new ProjectRepository(projectContext);
                });
            services.AddCap(options =>
            {
                //docker��װRabbitMQ��docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3-management
                options.UseEntityFramework<ProjectContext>()
                    .UseRabbitMQ(options =>
                    {
                        options.HostName = Configuration["Cap:MqHost"];
                        options.UserName = Configuration["Cap:MqUserName"];
                        options.Password = Configuration["Cap:MqPassword"];

                    })
                    .UseDashboard();

                //�����ֵķ������ڵ�
                options.UseDiscovery(d =>
                {
                    d.DiscoveryServerHostName = Configuration["ServiceDiscovery:Consul:DnsEndpoint:Address"];
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = Configuration["LocalService:HttpHost"];
                    d.CurrentNodePort = Convert.ToInt32(Configuration["LocalService:HttpPort"]);
                    d.NodeId = Configuration["Cap:ConsulNodeId"];
                    d.NodeName = "CAP Project API Node";
                });
            });
            services.AddMediatR();
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

            app.UseAuthentication(); //��ȡclaims�Ĺؼ�
            app.UseAuthorization();
            app.UseCapDashboard();
            //app.UseCap();
            //���÷���ע���ڷ���
            app.UseConsul();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
