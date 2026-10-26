using EcommerceApp.Extensions;
namespace EcommerceApp.Models;
// Version améliorée de ListeCart : un panier par session (et non un singleton partagé)
public class ListeCart
{
    private const string Key = "PANIER";
    private readonly ISession session;
    public List<Item> Items { get; private set; }

    public ListeCart(ISession session)
    {
        this.session = session;
        Items = session.GetJson<List<Item>>(Key) ?? new List<Item>();
    }
    private void Save() => session.SetJson(Key, Items);

    public void AddItem(Product prod, int qty = 1)
    {
        var it = Items.FirstOrDefault(i => i.ProductId == prod.ProductId);
        if (it != null) it.quantite = Math.Min(it.quantite + qty, Math.Max(prod.QteStock, 1));
        else Items.Add(new Item { ProductId = prod.ProductId, Name = prod.Name, Price = prod.Price, Image = prod.Image, CategoryName = prod.Category?.CategoryName, quantite = qty });
        Save();
    }
    public void SetLessOneItem(int id)
    {
        var it = Items.FirstOrDefault(i => i.ProductId == id);
        if (it == null) return;
        it.quantite--; if (it.quantite <= 0) Items.Remove(it);
        Save();
    }
    public void SetItemQuantity(int id, int q)
    {
        var it = Items.FirstOrDefault(i => i.ProductId == id);
        if (it == null) return;
        if (q <= 0) Items.Remove(it); else it.quantite = q;
        Save();
    }
    public void RemoveItem(int id) { Items.RemoveAll(i => i.ProductId == id); Save(); }
    public void Clear() { Items.Clear(); Save(); }
    public float GetSubTotal() => Items.Sum(i => i.TotalPrice);
    public int Count => Items.Sum(i => i.quantite);
}
