using EcommerceApp.Models;
using EcommerceApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace EcommerceApp.Controllers;
// Charge automatiquement les catégories (menu latéral) et le compteur panier pour chaque vue
public abstract class BaseShopController : Controller
{
    protected readonly ICategoryRepository CategRepository;
    protected BaseShopController(ICategoryRepository c) => CategRepository = c;
    protected ListeCart Cart => new ListeCart(HttpContext.Session);
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        ViewData["Categories"] = CategRepository.GetAll();
        ViewData["CartCount"] = Cart.Count;
        base.OnActionExecuting(ctx);
    }
}
