using Microsoft.EntityFrameworkCore;
using Pluralsight.CleanArchitecture.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Lab environment specific: this stops .NET from automatically blocking iframes.
builder.Services.AddAntiforgery(options =>
{
    options.SuppressXFrameOptionsHeader = true;
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RecipeCatalogDbContext>(options =>
    options.UseSqlite("Data Source=recipecatalog.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RecipeCatalogDbContext>();
    context.Database.EnsureCreated();
}

// Lab environment specific: set the security headers.
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'self' https://app.pluralsight.com");
    context.Response.Headers.Remove("X-Frame-Options");
    await next();
});

Directory.CreateDirectory("notifications");

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Recipes}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
