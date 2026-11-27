var builder = WebApplication.CreateBuilder(args);
// HARIA-WEB: CONFIGURATION BASE
builder.Configuration.Sources.Insert(0, new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource { Path = "appsettings.Haria.json", Optional = false, ReloadOnChange = true, FileProvider = builder.Environment.ContentRootFileProvider });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CraftCaster.Web.Services.MotorPartidaService>();
// HARIA-WEB: LOCALIZATION SERVICES
CraftCaster.Web.Utils.Localization.HariaWebLocalization.AddHariaWebLocalization(builder.Services, builder.Configuration);
// HARIA-WEB: USER SESSION SERVICES
CraftCaster.Web.Utils.Security.HariaWebClassSession.AddHariaWebClassSession(builder.Services);
// HARIA-WEB: SECURITY BEGIN
CraftCaster.Web.Configuration.HariaApplicationRegistration.AddHariaApplication(builder.Services, builder.Configuration, builder.Environment);
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(cookie =>
    {
        cookie.LoginPath = "/Cuenta/Acceso";
        cookie.AccessDeniedPath = "/Account/AccessDenied";
        cookie.Cookie.HttpOnly = true;
        cookie.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        cookie.Cookie.SameSite = SameSiteMode.Lax;
        cookie.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
// HARIA-WEB: SECURITY END

var app = builder.Build();
// HARIA-WEB: LOCALIZATION MIDDLEWARE
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// HARIA-WEB: USER SESSION MIDDLEWARE
app.UseSession();
app.UseRouting();

// HARIA-WEB: SECURITY MIDDLEWARE BEGIN
app.UseAuthentication();
// HARIA-WEB: SECURITY MIDDLEWARE END
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
