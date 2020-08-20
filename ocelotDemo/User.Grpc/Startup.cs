using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ST.Common.Consul;
using ST.Common.MagicOnion;

namespace User.Grpc
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
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();
            //添加rpc
            services.AddGrpc(Configuration.GetSection("RpcDiscovery"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //Bootstrap.Initialize(Configuration);
            //Bootstrap.Start();
            app.UseRouting();
           
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseConsul();
            //注册和启动rpc服务
            app.UseRpc();
        }
    }
}
