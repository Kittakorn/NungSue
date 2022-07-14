using AspNetCoreHero.ToastNotification.Extensions;
using FluentValidation.AspNetCore;
using NungSue.Extensions;
using NungSue.Validators;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigBlobService(builder.Configuration);
builder.Services.ConfigeDatabase(builder.Configuration);
builder.Services.ConfigAuthentication(builder.Configuration);
builder.Services.ConfigNotify();
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews().AddFluentValidation(options =>
{
    options.RegisterValidatorsFromAssemblyContaining<SetPasswordValidator>();
}); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var cultureInfo = new CultureInfo("en-US");

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseNotyf();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
