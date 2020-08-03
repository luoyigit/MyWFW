using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var config = new ConfigurationBuilder()
.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
.Build();
            var url = $"http://{config["LocalService:HttpHost"]}:{config["LocalService:HttpPort"]}";
            CreateHostBuilder(args, url).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string startUrl) =>
           Host.CreateDefaultBuilder(args)
          .ConfigureAppConfiguration((hostBuilderContext, builder) =>
          {
              builder.SetBasePath(hostBuilderContext.HostingEnvironment.ContentRootPath)
                  .AddJsonFile("Ocelot.json", false, true);
          })
          .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseUrls(startUrl).UseStartup<Startup>(); });
    }
}
