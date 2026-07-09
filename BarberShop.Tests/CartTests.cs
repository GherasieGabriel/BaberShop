using BarberShop.Models;
using BarberShop.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BarberShop.Tests;

public class CartTests
{
    [Fact]
    public void ShoppingCart_AddUpdateRemoveAndClear_Work()
    {
        var cart = new ShoppingCart();

        cart.AddItem("1", "Pomade", 25m, 2);
        cart.AddItem("1", "Pomade", 25m, 1);
        cart.AddItem("2", "Shampoo", 15m, 3);
        cart.UpdateQuantity("2", 1);
        cart.RemoveItem("1");

        Assert.Single(cart.Items);
        Assert.Equal("2", cart.Items[0].Id);
        Assert.Equal(1, cart.GetItemCount());
        Assert.Equal(15m, cart.GetTotal());

        cart.Clear();

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void CartService_UsesSessionForRoundTrip()
    {
        var service = new CartService();
        var context = new DefaultHttpContext();
        context.Session = new TestSession();

        service.AddToCart(context, "1", "Pomade", 25m, 2);
        service.UpdateQuantity(context, "1", 4);
        service.AddToCart(context, "2", "Shampoo", 15m, 1);

        var cart = service.GetCart(context);

        Assert.Equal(2, cart.Items.Count);
        Assert.Equal(5, cart.GetItemCount());
        Assert.Equal(115m, cart.GetTotal());

        service.RemoveFromCart(context, "2");
        cart = service.GetCart(context);

        Assert.Single(cart.Items);
        Assert.Equal("1", cart.Items[0].Id);

        service.ClearCart(context);
        cart = service.GetCart(context);

        Assert.Empty(cart.Items);
    }
}
