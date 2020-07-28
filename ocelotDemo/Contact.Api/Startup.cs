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
            //����MongoDb����
            services.Configure<AppSetting>(Configuration);
            ///���ӷ�����  �������ð�    
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
                options.SwaggerDoc("contactApi", new OpenApiInfo() { Title = "Contact API �ӿ�", Version = "v1" });
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

            //����ע��
            services.AddScoped(typeof(ContactContext));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>();
            services.AddScoped<IContactBookRepository, MongoContactBookRepository>();

            services.AddControllers();


            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:9000";
                    //options.Authority = "http://localhost:61114"; //һ��д���ص�ַ����ת������Ϊindentity server �����ж����
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "contact_api";

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