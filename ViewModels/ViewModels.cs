using System.ComponentModel.DataAnnotations;
using EcommerceApp.Models;
namespace EcommerceApp.ViewModels;

public class ProduitPaginationViewModel
{
    public List<Product> Products { get; set; }
    public int PageActuelle { get; set; }
    public int TotalPages { get; set; }
    public int TotalProducts { get; set; }
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Search { get; set; }
    public string Sort { get; set; }
}

public class OrderItemViewModel
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public float Price { get; set; }
}

public class OrderViewModel
{
    [Required(ErrorMessage = "Adresse obligatoire")] public string Address { get; set; }
    [Required(ErrorMessage = "Téléphone obligatoire"), Phone] public string Phone { get; set; }
    [Required] public string PaymentMethod { get; set; } = "Carte";
    public List<OrderItemViewModel> Items { get; set; } = new();
    public float TotalAmount { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string ReturnUrl { get; set; }
}
public class RegisterViewModel
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(4), DataType(DataType.Password)] public string Password { get; set; }
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas"), DataType(DataType.Password)] public string ConfirmPassword { get; set; }
}
