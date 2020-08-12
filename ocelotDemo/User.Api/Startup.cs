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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ST.Common.Consul;
using User.Api.Authorization;
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
                options.Filters.Add(new TestAuthorizationFilter());
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

            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddIdentityServerAuthentication(options =>
            //    {
            //        //options.Authority = "http://localhost:9000"; //
            //        options.Authority = Configuration["Gateway:Address"]; //һ��д���ص�ַ����ת������Ϊindentity server �����ж����//�����Ҫ������token ��ַһ�²����õ�claims
            //        options.RequireHttpsMetadata = false;
            //        //options.ApiName = "user_api";
            //        options.ApiName = Configuration["IndetityServer:ApiName"];

            //    });
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

            services.AddCap(options =>
            {
                //docker��װRabbitMQ��docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3-management
                options.UseEntityFramework<UserContext>()
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
                    d.DiscoveryServerHostName = "192.168.1.165";
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = Configuration["LocalService:HttpHost"];
                    d.CurrentNodePort = Convert.ToInt32(Configuration["LocalService:HttpPort"]);
                    d.NodeId = Configuration["LocalService:HostTag"];
                    d.NodeName = "CAP User API Node";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env
            )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication(); //��ȡclaims�Ĺؼ�
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
