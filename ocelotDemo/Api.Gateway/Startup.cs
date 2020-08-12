using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace Api.Gateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //添加IdentityServer4身份认证
            //IdentityServer4.AccessTokenValidation
            //services.AddAuthentication()
            //    .AddIdentityServerAuthentication("TestKey", options =>
            //    {
            //        options.Authority = Configuration["IndetityServer:Address"];
            //        options.ApiName = Configuration["IndetityServer:ApiName"];
            //        options.SupportedTokens = SupportedTokens.Both;
            //        options.ApiSecret = Configuration["IndetityServer:Secret"];
            //        options.RequireHttpsMetadata = false;
            //    });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 //IdentityServer地址
                 options.Authority = Configuration["IndetityServer:Address"];
                 //对应Idp中ApiResource的Name
                 //options.Audience = Configuration["IndetityServer:ApiName"];
                 //不使用https
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateAudience = false, //如果没有这个，所有都是401
                                               //ValidateIssuer = false
                   };
             });

            services.AddOcelot().AddConsul();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync($"Hello World!{Configuration["LocalService:HostTag"]}");
                });
            });
            //app.UseOcelot();
            app.UseAuthentication()
            .UseOcelot()
            .Wait();
        }
    }
}
