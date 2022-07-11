using AspNetCoreHero.ToastNotification;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using NungSue.Constants;
using NungSue.Entities;
using NungSue.Interfaces;
using NungSue.Services;

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
            services.AddDbContext<BookStoreContext>(options => options.UseSqlServer(connectionString));
        }

        public static void ConfigAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddCookie(AuthSchemes.ExternalAuth)
                .AddCookie(AuthSchemes.UserAuth, AuthSchemes.UserAuth, option =>
                {
                    option.LoginPath = new PathString("/Admin/SignIn");
                    option.AccessDeniedPath = "/Error/UnAuthorized";
                    option.ExpireTimeSpan = TimeSpan.FromDays(1);
                })
                .AddCookie(AuthSchemes.CustomerAuth, AuthSchemes.CustomerAuth, option =>
                {
                    option.LoginPath = new PathString("/account/sign-in");
                    option.AccessDeniedPath = "/Error/UnAuthorized";
                    option.ExpireTimeSpan = TimeSpan.FromDays(1);
                })
                .AddFacebook(option =>
                {
                    option.SignInScheme = AuthSchemes.ExternalAuth;
                    option.AppId = configuration["Authentication:Facebook:AppId"];
                    option.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                    option.CallbackPath = "/account/signin-facebook";
                })
                .AddGoogle(option =>
                {
                    option.SignInScheme = AuthSchemes.ExternalAuth;
                    option.ClientId = configuration["Authentication:Google:ClientId"];
                    option.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                    option.CallbackPath = "/account/signin-google";
                });
        }

    }
}