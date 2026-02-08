using Lihtar.Application.Interfaces;
using Lihtar.Application.Services;
using Lihtar.Infrastructure.Data;
using Lihtar.Infrastructure.Identity;
using Lihtar.Infrastructure.Repositories;
using Lihtar.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();

builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();

builder.Services.AddDbContext<ArtPubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ArtPubDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.SignIn.RequireConfirmedEmail = true;


});


builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/Denied";
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArtPubDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await DbInitializer.SeedAsync(db, userManager, roleManager);
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/Admin", ctx =>
{
    ctx.Response.Redirect("/Admin/AdminHome");
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
