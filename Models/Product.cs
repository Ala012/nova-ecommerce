using System.ComponentModel.DataAnnotations;
namespace EcommerceApp.Models;
public class Product
{
    public int ProductId { get; set; }
    [Required, StringLength(120)] public string Name { get; set; }
    [StringLength(2000)] public string Description { get; set; }
    [Range(0.01, 1000000)] public float Price { get; set; }
    [Range(0, 1000000)] public int QteStock { get; set; }
    [Url, StringLength(2048)] public string Image { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez une catégorie.")] public int CategoryId { get; set; }
    public Category Category { get; set; }
}
