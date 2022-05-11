using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CronoLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CodeGen.GenAll();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
#if DEBUG

#else
                    var portEnv = Environment.GetEnvironmentVariable("PORT");
                    int port = portEnv is not null ? int.Parse(portEnv) : 5000;
                    webBuilder.UseUrls($"http://*:{port}");
#endif
                    webBuilder.UseStartup<Startup>();
                });
    }
}
