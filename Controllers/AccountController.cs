using EcommerceApp.Repositories;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace EcommerceApp.Controllers;
public class AccountController : BaseShopController
{
    private readonly UserManager<IdentityUser> um; private readonly SignInManager<IdentityUser> sm;
    public AccountController(UserManager<IdentityUser> u, SignInManager<IdentityUser> s, ICategoryRepository c) : base(c) { um = u; sm = s; }

    public IActionResult Login(string returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await sm.PasswordSignInAsync(m.Email, m.Password, m.RememberMe, false);
        if (r.Succeeded) return LocalRedirect(Url.IsLocalUrl(m.ReturnUrl) ? m.ReturnUrl : "/");
        ModelState.AddModelError("", "Email ou mot de passe incorrect.");
        return View(m);
    }
    public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var user = new IdentityUser { UserName = m.Email, Email = m.Email };
        var r = await um.CreateAsync(user, m.Password);
        if (r.Succeeded)
        {
            var roleResult = await um.AddToRoleAsync(user, "Customer");
            if (roleResult.Succeeded)
            {
                await sm.SignInAsync(user, false);
                return RedirectToAction("Index", "Product");
            }

            var cleanupResult = await um.DeleteAsync(user);
            if (!cleanupResult.Succeeded)
                throw new InvalidOperationException($"L’attribution du rôle client a échoué et le compte créé n’a pas pu être annulé : {string.Join(", ", cleanupResult.Errors.Select(e => e.Description))}");
            foreach (var e in roleResult.Errors) ModelState.AddModelError("", e.Description);
            return View(m);
        }
        foreach (var e in r.Errors) ModelState.AddModelError("", e.Description);
        return View(m);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await sm.SignOutAsync(); return RedirectToAction("Index", "Product"); }
}
