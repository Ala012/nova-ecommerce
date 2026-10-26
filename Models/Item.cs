namespace EcommerceApp.Models;
// Élément du panier (TP4) — stocké en session par utilisateur
public class Item
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public float Price { get; set; }
    public string Image { get; set; }
    public string CategoryName { get; set; }
    public int quantite { get; set; }
    public float TotalPrice => Price * quantite;
}
