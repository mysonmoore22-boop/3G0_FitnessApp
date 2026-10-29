using FitnessApp.Helpers;
using FitnessApp.Models;
using FitnessApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;

namespace FitnessApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Gives the static AppConfiguration class access to appsettings.json
            // (also triggers its static constructor, which registers the Dapper maps)
            object Auth = AppConfiguration.Initialize(builder.Configuration);

            // Add services to the container.
            builder.Services.AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
            }).AddSessionStateTempDataProvider();

            builder.Services.AddSession();

            builder.Services.AddDbContext<AppIdentityDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppIdentityDBContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Identity already registers the cookie schemes and defaults. Only add Google.
            //builder.Services.AddAuthentication()
            //    .AddGoogle("Google", options =>
            //    {
            //        options.ClientId = .Configuration["Authentication:Google:ClientId"]!;
            //        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            //    });

            builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            builder.Services.Configure<FormOptions>(x =>
            {
                x.MemoryBufferThreshold = Int32.MaxValue;
                x.MultipartBodyLengthLimit = long.MaxValue;
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
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

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