using ASassn2_233104M.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
    });
builder.Services.AddHttpContextAccessor();

// Add Antiforgery Protection
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.HeaderName = "X-CSRF-TOKEN"; // Use header-based CSRF token validation
});

// Add HttpContextAccessor for session access in views
builder.Services.AddHttpContextAccessor();

// Add HttpClient support for external API calls (e.g., Google reCAPTCHA)
builder.Services.AddHttpClient();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session Support
builder.Services.AddDistributedMemoryCache(); // Required for session storage
builder.Services.AddSession(options =>
{
    //options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.IdleTimeout = TimeSpan.FromMinutes(5); // Set session timeout to 5 minutes
    options.Cookie.HttpOnly = true; // Prevents JavaScript access
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only allow HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevents cross-site requests
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// EnableAntiforgery Middleware
app.Use(next => context =>
{
    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
    var tokens = antiforgery.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
    {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.Strict
    });
    return next(context);
});

// Enable Session Middleware
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();