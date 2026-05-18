using BarberShop.Models;
using System.Text.Json;

namespace BarberShop.Services;

public interface ICartService
{
    ShoppingCart GetCart(HttpContext httpContext);
    void SaveCart(HttpContext httpContext, ShoppingCart cart);
    void AddToCart(HttpContext httpContext, string id, string name, decimal price, int quantity = 1);
    void RemoveFromCart(HttpContext httpContext, string id);
    void UpdateQuantity(HttpContext httpContext, string id, int quantity);
    void ClearCart(HttpContext httpContext);
}

public class CartService : ICartService
{
    private const string CartSessionKey = "ShoppingCart";

    public ShoppingCart GetCart(HttpContext httpContext)
    {
        var session = httpContext.Session;
        var cartJson = session.GetString(CartSessionKey);

        if (string.IsNullOrEmpty(cartJson))
        {
            return new ShoppingCart();
        }

        return JsonSerializer.Deserialize<ShoppingCart>(cartJson) ?? new ShoppingCart();
    }

    public void SaveCart(HttpContext httpContext, ShoppingCart cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        httpContext.Session.SetString(CartSessionKey, cartJson);
    }

    public void AddToCart(HttpContext httpContext, string id, string name, decimal price, int quantity = 1)
    {
        var cart = GetCart(httpContext);
        cart.AddItem(id, name, price, quantity);
        SaveCart(httpContext, cart);
    }

    public void RemoveFromCart(HttpContext httpContext, string id)
    {
        var cart = GetCart(httpContext);
        cart.RemoveItem(id);
        SaveCart(httpContext, cart);
    }

    public void UpdateQuantity(HttpContext httpContext, string id, int quantity)
    {
        var cart = GetCart(httpContext);
        cart.UpdateQuantity(id, quantity);
        SaveCart(httpContext, cart);
    }

    public void ClearCart(HttpContext httpContext)
    {
        httpContext.Session.Remove(CartSessionKey);
    }
}
