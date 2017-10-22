using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using XmlCombiner.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace XmlCombiner.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            Migrate(host.Services);

            host.Run();
        }

        public static void Migrate(IServiceProvider services)
        {
            bool migrated = false;
            int attempts = 0;
            while (!migrated)
            {
                try
                {
                    using (var scope = services.CreateScope())
                    {
                        var ctx = scope.ServiceProvider.GetRequiredService<XmlCombinerContext>();
                        ctx.Database.EnsureCreated();
                        ctx.Database.Migrate();
                        migrated = true;
                    }
                }
                catch (Exception) when (attempts < 5)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    attempts += 1;
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
