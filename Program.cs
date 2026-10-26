using EcommerceApp.Data;
using EcommerceApp.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequiredLength = 4;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromHours(2); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // crée la base MySQL + tables au premier lancement
    SeedData.Seed(db);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    await SeedData.SeedRolesAndConfiguredAdminAsync(roleManager, userManager, builder.Configuration);
}

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Product}/{action=Index}/{id?}");
app.Run();
