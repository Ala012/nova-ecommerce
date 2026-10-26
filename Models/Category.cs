using System.ComponentModel.DataAnnotations;
namespace EcommerceApp.Models;
public class Category
{
    public int CategoryId { get; set; }
    [Required, StringLength(80)] public string CategoryName { get; set; }
    [Required, StringLength(8)] public string Icon { get; set; } = "◆";
    public List<Product> Products { get; set; } = new();
}
