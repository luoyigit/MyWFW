using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resilience;
using ST.Common.Consul;
using User.Identity.Authentication;
using User.Identity.Infrastructure;
using User.Identity.Services;

namespace User.Identity
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
            services.AddTransient<IProfileService, UserProfileService>();

            //ע��Application Service
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //ע��IResilientHttpClientFactory
            services.AddSingleton<IResilientHttpClientFactory, ResilientHttpClientFactory>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilientHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                //�������Դ���
                var retryCount = 6;
                //�����ڶ�·������ǰ������ʧ�ܴ���
                var exceptionAllowBeforeBreaking = 5;
                return new ResilientHttpClientFactory(logger, httpContextAccessor, retryCount, exceptionAllowBeforeBreaking);
            });

            //ע��IHttpClient ��ResilientHttpClientʵ��
            services.AddSingleton<IHttpClient, ResilientHttpClient>(sp => sp.GetService<IResilientHttpClientFactory>().CreateResilientHttpClient());

            //services.AddSingleton<HttpClient>(new HttpClient());
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthCodeService, TestAuthCodeService>();

            //���ӷ�����  �������ð�             
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();

            services.AddIdentityServer()
               .AddExtensionGrantValidator<SmsAuthCodeValidator>()
               .AddDeveloperSigningCredential()
               .AddInMemoryApiResources(Config.GetApiResources())
               .AddInMemoryClients(Config.GetClients())
               .AddInMemoryIdentityResources(Config.GetIdentityResources());
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
            //���÷���ע���ڷ���
            app.UseConsul();

            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}