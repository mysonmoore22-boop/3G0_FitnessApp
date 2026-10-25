using FitnessApp.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FitnessApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
            }).AddSessionStateTempDataProvider();

            builder.Services.AddSession();
            builder.Services.AddIdentity<IdentityUser, IdentityRole>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie().AddGoogle("Google", options =>
            {
                //options.ClientId = My client id goes here ex: "2049284928498394809.apps.googleusercontent.com";
                //options.ClientSecret = "my client secret"

            });

            builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
            //builder.Services.AddHttpClient < EGO_BLL.> ();
            //Basically you should be adding your services here. AHP did this with their APIs. 
            //builder.Services.AddSingleton<IPathProvider, PathProvider>();

            builder.Services.Configure<FormOptions>(x =>
            {
                x.MemoryBufferThreshold = Int32.MaxValue;
                x.MultipartBodyLenghtLimit = long.MaxValue;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapStaticAssets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();
            });
            app.Run();
        }
    }
}



