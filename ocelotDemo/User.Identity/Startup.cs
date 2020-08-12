using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DnsClient;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using ST.Common.Consul;
using User.Identity.Authentication;
using User.Identity.Infrastructure;
using User.Identity.Models;
using User.Identity.Services;
using IdentityServer4.EntityFramework;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.IO;
using System.Security.Cryptography.X509Certificates;
namespace User.Identity
{
    public class Startup
    {
      
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            //var fileName = Path.Combine(_hostingEnvironment.ContentRootPath, "tempkey.rsa");

            //if (!File.Exists(fileName))
            //{
            //    throw new FileNotFoundException("Signing Certificate is missing!");
            //}

            //var cert = new X509Certificate2(fileName, "123456");

            //services.AddIdentityServer().AddSigningCredential(cert)
            var builder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = Configuration["Gateway:Address"];
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                //options.EmitStaticAudienceClaim = true;
            })
              // this adds the config data from DB (clients, resources, CORS)
              .AddConfigurationStore(options =>
              {
                  options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
              })
              // this adds the operational data from DB (codes, tokens, consents)
              .AddOperationalStore(options =>
              {
                  options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
              });

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential()//把tempky.rsa 始终复制，否则多台就会有问题。
            //builder.AddSigningCredential(cert)
            .AddExtensionGrantValidator<SmsAuthCodeValidator>();
           

            //  services.AddIdentityServer(options =>
            //  {
            //      options.IssuerUri = Configuration["Gateway:Address"];
            //  })
            // .AddExtensionGrantValidator<SmsAuthCodeValidator>()
            // .AddInMemoryIdentityResources(Config.GetIdentityResources())
            ////.AddInMemoryApiScopes(Config.GetApiScopes())
            //.AddDeveloperSigningCredential()
            //.AddInMemoryApiResources(Config.GetApiResources())
            //.AddInMemoryClients(Config.GetClients());




            services.AddTransient<IProfileService, UserProfileService>();

            //注入Application Service
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

            //services.AddSingleton<HttpClient>(new HttpClient());
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthCodeService, TestAuthCodeService>();

            //添加服务发现  进行配置绑定             
            services.AddConsulClient(Configuration.GetSection("ServiceDiscovery"))
                .AddDnsClient();



            //services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            //services.AddSingleton<IDnsQuery>(p =>
            //{
            //    var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
            //    return new LookupClient(serviceConfiguration.Consul.DnsEndPoint.ToIpEndPoint());
            //});
          
            services.AddControllers();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                //if (!context.Clients.Any())
                //{
                    foreach (var client in Config.GetClients())
                    {
                        if(context.Clients.FirstOrDefault(m=>m.ClientId == client.ClientId) == null)
                        {
                           context.Clients.Add(client.ToEntity());
                        }
                    }
                    context.SaveChanges();
                //}

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                //if (!context.ApiResources.Any())
                //{
                    foreach (var resource in Config.GetApiResources())
                    {
                        if(context.ApiResources.FirstOrDefault(m=>m.Name == resource.Name) == null)
                        {
                            context.ApiResources.Add(resource.ToEntity());
                        }
                       
                    }
                    context.SaveChanges();
                //}

                foreach(var apiScope in Config.GetApiScopes())
                {
                    if(context.ApiScopes.FirstOrDefault(m=>m.Name == apiScope.Name) == null)
                    {
                        context.ApiScopes.Add(apiScope.ToEntity());
                    }
                }
                context.SaveChanges();
            }

        }
            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            //启用服务注册于发现
            app.UseConsul();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SeedData.EnsureSeedData(Configuration.GetConnectionString("DefaultConnection"));
            InitializeDatabase(app);
        }
    }
}
