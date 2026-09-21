using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Infrastructure;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Web.Middlewares;
using SodiqCafeMVC.Web.Services;
using SodiqCafeMVC.Web.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Infrastructure qatlamini ulash (DbContext, TokenService, AuthService)
builder.Services.AddInfrastructure(builder.Configuration);

// Web servislar
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddHttpContextAccessor();

// SignalR
builder.Services.AddSignalR();

// JWT Authentication sozlamalari
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "SodiqCafe_Super_Secret_Jwt_Security_Key_For_Tokens_2026_@#!$";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "SodiqCafe";
var audience = builder.Configuration["Jwt:Audience"] ?? "SodiqCafeUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        // HttpOnly cookiedan yoki QueryStringdan (SignalR uchun) token olish
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/nfc"))
            {
                context.Token = accessToken;
            }
            else if (context.Request.Cookies.TryGetValue(CookieService.AccessTokenCookieName, out var token) && !string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Agar API so'rovi bo'lmasa, MVC sahifalar uchun login sahifasiga redirect qiladi
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                context.HandleResponse();
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/auth/login?returnUrl={returnUrl}");
            }
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Redirect("/auth/access-denied");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

// Ma'lumotlar bazasi va dastlabki adminni yaratish
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.EnsureSeedAdminAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// HttpOnly Cookie orqali tokenni avtomatik yangilash middleware
app.UseMiddleware<TokenRefreshMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapHub<NfcHub>("/hubs/nfc");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
