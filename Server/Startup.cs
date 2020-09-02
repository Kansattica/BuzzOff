using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using BuzzOff.Server.Hubs;
using Microsoft.AspNetCore.StaticFiles;
using System;
using Microsoft.ApplicationInsights;
using CompressedStaticFiles;

namespace BuzzOff.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute("/Index", "/Room");
            });
            services.AddResponseCompression(opts =>
            {
                opts.EnableForHttps = true;
                opts.Providers.Add<GzipCompressionProvider>();
                opts.Providers.Add<BrotliCompressionProvider>();
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddSingleton<RoomManager>();
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365 * 10);
            });

            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            
            // Try serving .gz'd static files if they exist
            // https://github.com/AnderssonPeter/CompressedStaticFiles
            app.UseCompressedStaticFiles(new StaticFileOptions() {
                ContentTypeProvider = provider
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<BuzzHub>("/buzz");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
