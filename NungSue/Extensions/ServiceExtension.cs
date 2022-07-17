using AspNetCoreHero.ToastNotification;
using Azure.Storage.Blobs;
using LineAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Interfaces;
using NungSue.Services;
using System.Security.Claims;

namespace NungSue.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigNotify(this IServiceCollection services)
        {
            services.AddNotyf(config =>
            {
                config.DurationInSeconds = 10;
                config.IsDismissable = true;
                config.Position = NotyfPosition.BottomRight;
            });
        }

        public static void ConfigBlobService(this IServiceCollection services, IConfiguration configuration)
        {
            var blobConnection = configuration.GetValue<string>("BlobConnection");
            services.AddSingleton(x => new BlobServiceClient(blobConnection));
            services.AddSingleton<IBlobService, BlobService>();
        }

        public static void ConfigeDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<NungSueContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        public static void ConfigAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(x => x.DefaultScheme = AuthSchemes.CustomerAuth)
                .AddCookie(AuthSchemes.ExternalAuth)
                .AddCookie(AuthSchemes.UserAuth, AuthSchemes.UserAuth, options =>
                {
                    options.LoginPath = new PathString("/Admin/SignIn");
                    options.AccessDeniedPath = "/Error/UnAuthorized";
                })
                .AddCookie(AuthSchemes.CustomerAuth, AuthSchemes.CustomerAuth, options =>
                {
                    options.LoginPath = new PathString("/account/sign-in");
                    options.AccessDeniedPath = "/Error/UnAuthorized";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                })
                .AddFacebook(options =>
                {
                    options.SignInScheme = AuthSchemes.ExternalAuth;
                    options.AppId = configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                    options.CallbackPath = "/account/signin-facebook";
                })
                .AddGoogle(options =>
                {
                    options.SignInScheme = AuthSchemes.ExternalAuth;
                    options.ClientId = configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                    options.ClaimActions.MapJsonKey(ClaimTypes.Actor, "picture");
                    options.CallbackPath = "/account/signin-google";
                })
                .AddLine(options =>
                 {
                     options.SignInScheme = AuthSchemes.ExternalAuth;
                     options.ClientId = configuration["Authentication:Line:ChannelId"];
                     options.ClientSecret = configuration["Authentication:Line:ChannelSecret"];
                     options.CallbackPath = "/account/signin-line";
                     options.Scope.Add("email");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                 });
        }
    }
}