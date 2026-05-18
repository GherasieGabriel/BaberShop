namespace BarberShop.Models;

public class CartItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public decimal Total => Price * Quantity;
}

public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new();

    public decimal GetTotal() => Items.Sum(i => i.Total);
    public int GetItemCount() => Items.Sum(i => i.Quantity);

    public void AddItem(string id, string name, decimal price, int quantity = 1)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem { Id = id, Name = name, Price = price, Quantity = quantity });
        }
    }

    public void RemoveItem(string id)
    {
        Items.RemoveAll(i => i.Id == id);
    }

    public void UpdateQuantity(string id, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.Quantity = quantity;
            if (item.Quantity <= 0)
                RemoveItem(id);
        }
    }

    public void Clear()
    {
        Items.Clear();
    }
}
