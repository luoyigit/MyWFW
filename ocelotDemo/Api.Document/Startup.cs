using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Document.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ST.Common.Consul;

namespace Api.Document
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
            services.AddControllers();
            //添加服务发现  进行配置绑定    
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ApiDocument", new OpenApiInfo { Title = "ApiDocument", Version = "v1" });
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //启用服务注册于发现
            app.UseConsul();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.ShowExtensions();
                //options.SwaggerEndpoint("/swagger/ApiDocument/swagger.json", "ApiDocument V1");
                options.SwaggerEndpoint("http://192.168.1.165:5001/swagger/userApi/swagger.json", "用户服务");
            });
        }
    }
}
