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
using Microsoft.AspNetCore.Http;

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
			services.AddRazorPages();
			services.AddResponseCompression(opts =>
			{
				opts.EnableForHttps = true;
				opts.Providers.Add<BrotliCompressionProvider>();
				opts.Providers.Add<GzipCompressionProvider>();
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
			services.AddCompressedStaticFiles(options => options.RemoveImageSubstitutionCostRatio());
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

            app.Use((context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

				// don't cache the main page or the room pages
				// those expect to go to the server every time to get a new name
				if (context.Request.Path.HasValue && (context.Request.Path.Value == "/" || context.Request.Path.Value.StartsWith("/Room/")))
					context.Response.Headers["Cache-Control"] = "no-store";
				return next();
            });

			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".webmanifest"] = "application/manifest+json";
			provider.Mappings[".ogg"] = "audio/ogg";

			// Try serving .gz'd and .br'd static files if they exist
			// https://github.com/AnderssonPeter/CompressedStaticFiles
			app.UseCompressedStaticFiles(new StaticFileOptions() {
				ContentTypeProvider = provider,
				OnPrepareResponse = ctx =>
				{
					// can't do the cachebusting thing as easily with static HTML files (help.html), so don't cache them as long
					// we do cachebusting with css and javascript, so might as well cache those forever
					// the sounds are relatively large and don't change, so cache those forever, too
					if (ctx.File.Name.EndsWith(".html"))
						ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=86400");
					else
						ctx.Context.Response.Headers.Append("Cache-Control", "public, immutable, max-age=31536000");
				}
			});

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapHub<BuzzHub>("/buzz");
				endpoints.MapFallbackToPage("/Index");
			});
		}
	}
}
