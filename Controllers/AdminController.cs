using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Repositories;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcommerceApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseShopController
{
    private readonly AppDbContext db;
    private readonly UserManager<IdentityUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IWebHostEnvironment environment;

    public AdminController(AppDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ICategoryRepository categories, IWebHostEnvironment environment)
        : base(categories)
    {
        this.db = db;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.environment = environment;
    }

    public async Task<IActionResult> Products()
    {
        var products = await db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> CreateProduct()
    {
        await PopulateCategoriesAsync();
        return View("ProductForm", new ProductEditViewModel());
    }

    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product == null) return NotFound();
        await PopulateCategoriesAsync();
        return View("ProductForm", ProductEditViewModel.FromProduct(product));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProduct(ProductEditViewModel model)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "La catégorie sélectionnée n’existe pas.");

        Product product = null;
        if (model.ProductId != 0)
        {
            product = await db.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();
        }

        if (model.ImageFile is { Length: > 0 })
        {
            var validationError = await GetImageValidationErrorAsync(model.ImageFile);
            if (validationError != null)
                ModelState.AddModelError(nameof(model.ImageFile), validationError);
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View("ProductForm", model);
        }

        var isNewProduct = product == null;
        product ??= new Product();
        var previousImage = product.Image;
        string newImagePath = null;
        string newImageUrl = null;

        if (model.ImageFile is { Length: > 0 })
        {
            var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
            var fileName = $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}{extension}";
            var imageDirectory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "images", "products");
            Directory.CreateDirectory(imageDirectory);
            newImagePath = Path.Combine(imageDirectory, fileName);
            newImageUrl = $"/images/products/{fileName}";
            await using (var destination = System.IO.File.Create(newImagePath))
            {
                await model.ImageFile.CopyToAsync(destination);
            }
        }

        product.Name = model.Name.Trim();
        product.Description = model.Description?.Trim();
        product.Price = model.Price;
        product.QteStock = model.QteStock;
        product.CategoryId = model.CategoryId;
        if (newImageUrl != null) product.Image = newImageUrl;
        if (isNewProduct) db.Products.Add(product);

        var saveSucceeded = false;
        try
        {
            await db.SaveChangesAsync();
            saveSucceeded = true;
        }
        finally
        {
            if (!saveSucceeded && newImagePath != null && System.IO.File.Exists(newImagePath))
                System.IO.File.Delete(newImagePath);
        }

        if (newImageUrl != null) DeleteUploadedImage(previousImage);
        TempData["Toast"] = "Produit enregistré.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product == null) return NotFound();
        var image = product.Image;
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        DeleteUploadedImage(image);
        TempData["Toast"] = "Produit supprimé.";
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Categories()
    {
        var categories = await db.Categories.Include(c => c.Products).OrderBy(c => c.CategoryName).ToListAsync();
        return View(categories);
    }

    public IActionResult CreateCategory() => View("CategoryForm", new CategoryEditViewModel());

    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View("CategoryForm", new CategoryEditViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Icon = category.Icon
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryEditViewModel model)
    {
        if (!ModelState.IsValid) return View("CategoryForm", model);

        Category category;
        if (model.CategoryId == 0)
        {
            category = new Category();
            db.Categories.Add(category);
        }
        else
        {
            category = await db.Categories.FindAsync(model.CategoryId);
            if (category == null) return NotFound();
        }

        category.CategoryName = model.CategoryName.Trim();
        category.Icon = model.Icon.Trim();
        await db.SaveChangesAsync();
        TempData["Toast"] = "Catégorie enregistrée.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null) return NotFound();
        if (category.Products.Count != 0)
        {
            TempData["AdminError"] = "Cette catégorie contient des produits ; déplacez-les ou supprimez-les avant de supprimer la catégorie.";
            return RedirectToAction(nameof(Categories));
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        TempData["Toast"] = "Catégorie supprimée.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Users()
    {
        var users = await userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var model = new AdminUsersViewModel();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            model.Users.Add(new AdminUserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Role = roles.Contains("Admin") ? "Admin" : "Customer"
            });
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetUserRole(string userId, string role)
    {
        if (role is not ("Admin" or "Customer") || !await roleManager.RoleExistsAsync(role))
            return BadRequest("Rôle invalide.");

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (user.Id == userManager.GetUserId(User))
        {
            TempData["AdminError"] = "Vous ne pouvez pas modifier votre propre rôle.";
            return RedirectToAction(nameof(Users));
        }

        var existingRoles = await userManager.GetRolesAsync(user);
        if (existingRoles.Contains(role)) return RedirectToAction(nameof(Users));
        if (existingRoles.Contains("Admin") && role != "Admin")
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
            {
                TempData["AdminError"] = "Le dernier administrateur ne peut pas être rétrogradé.";
                return RedirectToAction(nameof(Users));
            }
        }

        if (existingRoles.Count != 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeResult.Succeeded)
            {
                TempData["AdminError"] = string.Join(" ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            if (existingRoles.Count != 0)
            {
                var restoreResult = await userManager.AddToRolesAsync(user, existingRoles);
                if (!restoreResult.Succeeded)
                    throw new InvalidOperationException($"Le changement de rôle a échoué et les rôles précédents n’ont pas pu être restaurés : {string.Join(", ", restoreResult.Errors.Select(e => e.Description))}");
            }
            TempData["AdminError"] = string.Join(" ", addResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Users));
        }

        await userManager.UpdateSecurityStampAsync(user);
        TempData["Toast"] = "Rôle utilisateur mis à jour.";
        return RedirectToAction(nameof(Users));
    }

    private static async Task<string> GetImageValidationErrorAsync(IFormFile image)
    {
        const long maxImageSize = 5 * 1024 * 1024;
        if (image.Length > maxImageSize) return "L’image ne doit pas dépasser 5 Mo.";

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            return "Format non accepté. Choisissez une image JPG, PNG, GIF ou WebP.";

        var header = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory());
        var isValidImage = extension switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".gif" => bytesRead >= 6 && (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
            ".webp" => bytesRead >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };

        return isValidImage ? null : "Le contenu du fichier ne correspond pas à une image valide de ce format.";
    }

    private void DeleteUploadedImage(string imageUrl)
    {
        const string imagePrefix = "/images/products/";
        if (imageUrl == null || !imageUrl.StartsWith(imagePrefix, StringComparison.Ordinal)) return;

        var fileName = imageUrl[imagePrefix.Length..];
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName)) return;

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var imagePath = Path.Combine(webRoot, "images", "products", fileName);
        if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
    }

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.ProductCategories = new SelectList(await db.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryId", "CategoryName");
    }
}
