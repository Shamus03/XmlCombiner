﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using XmlCombiner.Web.Infrastructure;

namespace XmlCombiner.Web
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
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "XmlCombiner API", Version = "v1" });
            });

            services.AddScoped<IFeedRepository, FeedRepository>();
            services.AddSingleton<FeedRepositoryOptions>();
            services.Configure<FeedRepositoryOptions>(c =>
            {
                string envFilePath = Environment.GetEnvironmentVariable("FEEDS_JSON");
                if (envFilePath != null)
                {
                    c.FilePath = envFilePath;
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "XmlCombiner V1");
            });

            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}
