using Microsoft.AspNetCore.Mvc;
namespace EcommerceApp.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Product");
    public IActionResult Error() => Content("Une erreur est survenue.");
}
