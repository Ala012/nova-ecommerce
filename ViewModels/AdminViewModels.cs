using System.ComponentModel.DataAnnotations;
using EcommerceApp.Models;

namespace EcommerceApp.ViewModels;

public class ProductEditViewModel
{
    public int ProductId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Range(0.01, 1000000)]
    public float Price { get; set; }

    [Range(0, 1000000)]
    public int QteStock { get; set; }

    public string Image { get; set; }

    public IFormFile ImageFile { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez une catégorie.")]
    public int CategoryId { get; set; }

    public static ProductEditViewModel FromProduct(Product product) => new()
    {
        ProductId = product.ProductId,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        QteStock = product.QteStock,
        Image = product.Image,
        CategoryId = product.CategoryId
    };
}

public class CategoryEditViewModel
{
    public int CategoryId { get; set; }

    [Required, StringLength(80)]
    public string CategoryName { get; set; }

    [Required, StringLength(8)]
    public string Icon { get; set; } = "◆";
}

public class AdminUsersViewModel
{
    public List<AdminUserRoleViewModel> Users { get; set; } = new();
}

public class AdminUserRoleViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
