using Lihtar.Application.Interfaces;
using Lihtar.Application.Services;
using Lihtar.Infrastructure.Data;
using Lihtar.Infrastructure.Identity;
using Lihtar.Infrastructure.Repositories;
using Lihtar.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Lihtar.Application.Validators.MenuCategoryDtoValidator).Assembly);
// DbContext
builder.Services.AddDbContext<ArtPubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();
builder.Services.AddScoped<IEventCategoryService, EventCategoryService>();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();

// ---------- REPOS + SERVICES ----------
builder.Services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();

builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();

builder.Services.AddScoped<IMenuItemTagRepository, MenuItemTagRepository>();
builder.Services.AddScoped<IMenuItemTagService, MenuItemTagService>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opt =>
{
    opt.SignIn.RequireConfirmedEmail = true;

    opt.Password.RequiredLength = 6;
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ArtPubDbContext>()
.AddDefaultTokenProviders();

// Cookies
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/Denied";
});

// Email
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddAutoMapper(typeof(Lihtar.Application.Mappings.MenuItemProfile).Assembly);
var app = builder.Build();

// ---------- SEED ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArtPubDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await DbInitializer.SeedAsync(db, userManager, roleManager);
}

// Middleware
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ---------- ROUTES ----------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AdminHome}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
