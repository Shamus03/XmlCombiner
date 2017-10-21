using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

            bool migrated = false;
            int attempts = 0;
            while (!migrated)
            {
                try
                {
                    using (var scope = host.Services.CreateScope())
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

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
