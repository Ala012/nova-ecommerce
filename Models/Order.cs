namespace EcommerceApp.Models;
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string PaymentMethod { get; set; }
    public string UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public float TotalAmount { get; set; }
    public string Status { get; set; } = "En préparation";
    public List<OrderItem> Items { get; set; } = new();
}
public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public float Price { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
}
