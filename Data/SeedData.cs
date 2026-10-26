using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity;
namespace EcommerceApp.Data;
public static class SeedData
{
    public static async Task SeedRolesAndConfiguredAdminAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        foreach (var roleName in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Impossible de créer le rôle {roleName} : {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        var email = configuration["AdminUser:Email"];
        var password = configuration["AdminUser:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password)) return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configurez AdminUser__Email et AdminUser__Password ensemble pour créer l’administrateur initial.");

        var admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var userResult = await userManager.CreateAsync(admin, password);
            if (!userResult.Succeeded)
                throw new InvalidOperationException($"Impossible de créer l’administrateur initial : {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Impossible d’attribuer le rôle Admin : {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }

    public static void Seed(AppDbContext db)
    {
        if (db.Categories.Any()) return;
        var alim = new Category { CategoryName = "Alimentaire", Icon = "🍫" };
        var hyg = new Category { CategoryName = "Hygiène", Icon = "🧴" };
        var meu = new Category { CategoryName = "Meuble", Icon = "🛋️" };
        var net = new Category { CategoryName = "Nettoyage", Icon = "🧽" };
        var vet = new Category { CategoryName = "Vêtement", Icon = "👕" };
        db.Categories.AddRange(alim, hyg, meu, net, vet);
        string img(string q) => $"https://images.unsplash.com/{q}?w=600&q=70&auto=format&fit=crop";
        db.Products.AddRange(
            new Product { Name = "Lilas Sol 1L", Price = 9.85f, QteStock = 10, Category = net, Description = "Nettoyant sols parfum lilas.", Image = img("photo-1585421514738-01798e348b17") },
            new Product { Name = "Javel Judy 1L", Price = 1.75f, QteStock = 200, Category = net, Description = "Eau de javel concentrée.", Image = img("photo-1583947215259-38e31be8751f") },
            new Product { Name = "Chocolat Maestro", Price = 4.5f, QteStock = 50, Category = alim, Description = "Chocolat au lait tunisien.", Image = img("photo-1606312619070-d48b4c652a52") },
            new Product { Name = "Après Shampooing", Price = 9.6f, QteStock = 10, Category = hyg, Description = "Soin démêlant cheveux.", Image = img("photo-1556228720-195a672e8a03") },
            new Product { Name = "Huile d'olive 1L", Price = 18.9f, QteStock = 40, Category = alim, Description = "Extra vierge, première pression.", Image = img("photo-1474979266404-7eaacbcd87c5") },
            new Product { Name = "Café moulu 250g", Price = 7.2f, QteStock = 60, Category = alim, Description = "Torréfaction artisanale.", Image = img("photo-1559056199-641a0ac8b55e") },
            new Product { Name = "Savon naturel", Price = 3.4f, QteStock = 80, Category = hyg, Description = "Savon à l'huile d'olive.", Image = img("photo-1600857544200-b2f666a9a2ec") },
            new Product { Name = "Chaise scandinave", Price = 129f, QteStock = 8, Category = meu, Description = "Bois de hêtre massif.", Image = img("photo-1503602642458-232111445657") },
            new Product { Name = "Table basse", Price = 245f, QteStock = 5, Category = meu, Description = "Chêne et métal noir.", Image = img("photo-1533090481720-856c6e3c1fdc") },
            new Product { Name = "T-shirt coton", Price = 29f, QteStock = 35, Category = vet, Description = "100% coton bio.", Image = img("photo-1521572163474-6864f9cf17ab") },
            new Product { Name = "Veste en jean", Price = 119f, QteStock = 12, Category = vet, Description = "Coupe classique délavée.", Image = img("photo-1551537482-f2075a1d41f2") },
            new Product { Name = "Éponges x5", Price = 2.9f, QteStock = 150, Category = net, Description = "Double face grattante.", Image = img("photo-1563453392212-326f5e854473") }
        );
        db.SaveChanges();
    }
}
