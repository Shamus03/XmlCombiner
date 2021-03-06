﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using XmlCombiner.Web.Infrastructure;

namespace XmlCombiner.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "XmlCombiner API", Version = "v1" });
            });

            if (Environment.IsDevelopment())
            {
                services.AddDbContext<XmlCombinerContext>(options => options.UseInMemoryDatabase(nameof(XmlCombinerContext)));
            }
            else
            {
                services.AddDbContext<XmlCombinerContext>(options => options.UseMySql(Configuration.GetConnectionString(nameof(XmlCombinerContext))));
            }
            services.AddTransient<IFeedGroupRepository, FeedGroupRepository>();
            services.AddTransient<IFeedRepository, FeedRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "XmlCombiner V1");
            });

            app.UseMvc();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
