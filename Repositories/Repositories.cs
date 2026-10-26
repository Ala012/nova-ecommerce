using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.EntityFrameworkCore;
namespace EcommerceApp.Repositories;

public interface ICategoryRepository { IList<Category> GetAll(); Category GetById(int id); }
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext context;
    public CategoryRepository(AppDbContext c) => context = c;
    public IList<Category> GetAll() => context.Categories.Include(c => c.Products).OrderBy(c => c.CategoryName).ToList();
    public Category GetById(int id) => context.Categories.Find(id);
}

public interface IProductRepository
{
    IQueryable<Product> GetAllProducts();
    Product GetById(int id);
    IList<Product> GetRelated(Product p, int n);
}
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext context;
    public ProductRepository(AppDbContext c) => context = c;
    public IQueryable<Product> GetAllProducts() => context.Products.Include(p => p.Category);
    public Product GetById(int id) => context.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
    public IList<Product> GetRelated(Product p, int n) => GetAllProducts().Where(x => x.CategoryId == p.CategoryId && x.ProductId != p.ProductId).Take(n).ToList();
}

public interface IOrderRepository { void Add(Order o); Order GetById(int id); IList<Order> GetByUser(string userId); }
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext context;
    public OrderRepository(AppDbContext c) => context = c;
    public void Add(Order o) { context.Orders.Add(o); context.SaveChanges(); }
    public Order GetById(int id) => context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);
    public IList<Order> GetByUser(string userId) => context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
}
