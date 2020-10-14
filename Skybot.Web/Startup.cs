using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Skybot.Web.Authorization;
using AspNet.Security.OAuth.Discord;

namespace Skybot.Web
{
    public class Startup
    {
        const string CORS_POLICY = "Skybot.Web.CorsPolicy";
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(o =>
            {
                o.CheckConsentNeeded = c => true;
                o.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddCors(o => o.AddPolicy(CORS_POLICY, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddScheme<ApiKeyOptions, ApiKeyHandler>(AuthenticationSchemes.ApiKeyScheme, o => { })
              .AddScheme<AdminKeyOptions, AdminKeyHandler>(AuthenticationSchemes.AdminScheme, o => { })
              .AddCookie(o => 
              {
                  o.LoginPath = "/Login";
                  o.AccessDeniedPath = "/Login";
                  o.ExpireTimeSpan = TimeSpan.FromHours(6);
              })
              .AddDiscord(o =>
              {
                  o.ClientId = SkyBot.SkyBotConfig.DiscordClientId;
                  o.ClientSecret = SkyBot.SkyBotConfig.DiscordClientSecret;
                  o.AccessDeniedPath = "/Login2";
                  o.Scope.Add("identify");
              });

            services.AddHttpClient();

            services.AddRazorPages(o =>
            {
                //Restrict any access by default
                o.Conventions.AuthorizeFolder("/");

                //Make sure we don't have to authorize api by user+pass
                o.Conventions.AllowAnonymousToFolder("/api/");

                //Allow anonymous access
                o.Conventions.AllowAnonymousToPage("/Index");

                //Allow login
                o.Conventions.AllowAnonymousToPage("/Login");

            }).AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCookiePolicy();
            app.UseCors(CORS_POLICY);
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
