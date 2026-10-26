using EcommerceApp.Models;
using EcommerceApp.Repositories;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace EcommerceApp.Controllers;
public class ProductController : BaseShopController
{
    private readonly IProductRepository ProductRepository;
    public ProductController(IProductRepository p, ICategoryRepository c) : base(c) => ProductRepository = p;

    public IActionResult Index(int? categoryId, string search, string sort, int page = 1)
    {
        int pageSize = 4;
        IQueryable<Product> q = ProductRepository.GetAllProducts();
        if (categoryId.HasValue) q = q.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.Name.Contains(search));
        q = sort switch
        {
            "prix_asc" => q.OrderBy(p => p.Price),
            "prix_desc" => q.OrderByDescending(p => p.Price),
            "nom" => q.OrderBy(p => p.Name),
            _ => q.OrderBy(p => p.ProductId)
        };
        int total = q.Count();
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)total / pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var vm = new ProduitPaginationViewModel
        {
            Products = q.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            PageActuelle = page, TotalPages = totalPages, TotalProducts = total,
            CategoryId = categoryId, Search = search, Sort = sort,
            CategoryName = categoryId.HasValue ? CategRepository.GetById(categoryId.Value)?.CategoryName : null
        };
        ViewBag.CategoryId = categoryId;
        return View(vm);
    }

    public IActionResult Details(int id)
    {
        var p = ProductRepository.GetById(id);
        if (p == null) return NotFound();
        ViewBag.Related = ProductRepository.GetRelated(p, 4);
        return View(p);
    }
}
