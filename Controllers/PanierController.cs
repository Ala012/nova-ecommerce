using EcommerceApp.Models;
using EcommerceApp.Repositories;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace EcommerceApp.Controllers;
public class PanierController : BaseShopController
{
    private readonly IProductRepository productRepository;
    private readonly IOrderRepository orderRepository;
    private readonly UserManager<IdentityUser> userManager;

    public PanierController(IProductRepository p, IOrderRepository o, UserManager<IdentityUser> u, ICategoryRepository c) : base(c)
    { productRepository = p; orderRepository = o; userManager = u; }

    public IActionResult Index() => View(Cart);

    // Ajout classique (lien) puis redirection vers le panier
    public IActionResult AddProduct(int id)
    {
        var prod = productRepository.GetById(id);
        if (prod == null) return NotFound();
        Cart.AddItem(prod);
        TempData["Toast"] = $"« {prod.Name} » ajouté au panier";
        return RedirectToAction(nameof(Index));
    }

    // Ajout AJAX (depuis la liste)
    [HttpPost]
    public IActionResult AddAjax(int id)
    {
        var prod = productRepository.GetById(id);
        if (prod == null) return NotFound();
        var cart = Cart; cart.AddItem(prod);
        return Json(new { Count = cart.Count, Total = cart.GetSubTotal(), Name = prod.Name });
    }

    [HttpPost]
    public IActionResult PlusProduct(int id)
    {
        var prod = productRepository.GetById(id);
        var cart = Cart; cart.AddItem(prod);
        return Json(Result(cart, id));
    }
    [HttpPost]
    public IActionResult MinusProduct(int id) { var cart = Cart; cart.SetLessOneItem(id); return Json(Result(cart, id)); }
    [HttpPost]
    public IActionResult RemoveProduct(int id) { var cart = Cart; cart.RemoveItem(id); return Json(Result(cart, id)); }
    [HttpPost]
    public IActionResult Vider() { Cart.Clear(); return RedirectToAction(nameof(Index)); }

    private static object Result(ListeCart cart, int id)
    {
        var t = cart.Items.FirstOrDefault(i => i.ProductId == id);
        return new { Total = cart.GetSubTotal(), Count = cart.Count, Quantite = t?.quantite ?? 0, TotalRow = t?.TotalPrice ?? 0 };
    }

    [Authorize]
    public IActionResult Checkout()
    {
        var cart = Cart;
        if (!cart.Items.Any()) return RedirectToAction(nameof(Index));
        return View(new OrderViewModel
        {
            Items = cart.Items.Select(i => new OrderItemViewModel { ProductName = i.Name, Quantity = i.quantite, Price = i.Price }).ToList(),
            TotalAmount = cart.GetSubTotal()
        });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(OrderViewModel model)
    {
        var cart = Cart;
        if (!cart.Items.Any()) { ModelState.AddModelError("", "Votre panier est vide."); }
        if (!ModelState.IsValid)
        {
            model.Items = cart.Items.Select(i => new OrderItemViewModel { ProductName = i.Name, Quantity = i.quantite, Price = i.Price }).ToList();
            model.TotalAmount = cart.GetSubTotal();
            return View(model);
        }
        var user = await userManager.GetUserAsync(User);
        var order = new Order
        {
            CustomerName = user.UserName, Email = user.Email, UserId = user.Id,
            Address = model.Address, Phone = model.Phone, PaymentMethod = model.PaymentMethod,
            OrderDate = DateTime.Now, TotalAmount = cart.GetSubTotal(),
            Items = cart.Items.Select(i => new OrderItem { ProductName = i.Name, Quantity = i.quantite, Price = i.Price }).ToList()
        };
        orderRepository.Add(order);
        cart.Clear();
        return RedirectToAction(nameof(Confirmation), new { orderId = order.Id });
    }

    [Authorize]
    public IActionResult Confirmation(int orderId)
    {
        var order = orderRepository.GetById(orderId);
        if (order == null || order.UserId != userManager.GetUserId(User)) return NotFound();
        return View(order);
    }
}
