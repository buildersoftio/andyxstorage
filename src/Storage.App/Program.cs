using Buildersoft.Andy.X.Storage.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Storage.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .CreateLogger();

            // SETTING environment variables for Env, Cert and default asp_net
            if (Environment.GetEnvironmentVariable("ANDYX_ENVIRONMENT") != null)
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environment.GetEnvironmentVariable("ANDYX_ENVIRONMENT"));

            if (Environment.GetEnvironmentVariable("ANDYX_CERTIFICATE_DEFAULT_PASSWORD") != null)
                Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password", Environment.GetEnvironmentVariable("ANDYX_CERTIFICATE_DEFAULT_PASSWORD"));

            if (Environment.GetEnvironmentVariable("ANDYX_CERTIFICATE_DEFAULT_PATH") != null)
                Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path", Environment.GetEnvironmentVariable("ANDYX_CERTIFICATE_DEFAULT_PATH"));

            // Commented this for know; found BUG, can not run on docker, is asking for certificate.
            //if (Environment.GetEnvironmentVariable("ANDYX_URLS") != null)
            //    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", Environment.GetEnvironmentVariable("ANDYX_URLS"));
            //else
            //    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "https://+:443;http://+:80");

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
