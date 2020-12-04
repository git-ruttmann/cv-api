namespace ruttmann.vita.api
{
    using System;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("UseAzureKeyVault")))
            {
                services.AddSingleton(typeof(IFileSystem), typeof(DiskFileSystem));
            }
            else
            {
                services.AddSingleton(typeof(IFileSystem), typeof(BlobFileSystem));
            }

            services.AddSingleton(typeof(IVitaDataService), typeof(VitaDataService));
            services.AddSingleton(typeof(ILinkedInOAuthService), typeof(LinkedInOAuthService));
            services.AddSingleton(typeof(IAuthService), typeof(VitaAuthService));
            services.AddSingleton(typeof(ITrackingService), typeof(TrackingService));
            services.AddSingleton(typeof(ITrackingReportMailer), typeof(TrackingReportMailer));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapDefaultControllerRoute();
            });

            // force the singleton to exist.
            app.ApplicationServices.GetService<ITrackingReportMailer>();
        }
    }
}
