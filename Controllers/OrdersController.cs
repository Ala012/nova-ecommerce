using EcommerceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace EcommerceApp.Controllers;
[Authorize]
public class OrdersController : BaseShopController
{
    private readonly IOrderRepository orders; private readonly UserManager<IdentityUser> um;
    public OrdersController(IOrderRepository o, UserManager<IdentityUser> u, ICategoryRepository c) : base(c) { orders = o; um = u; }
    public IActionResult Index() => View(orders.GetByUser(um.GetUserId(User)));
}
