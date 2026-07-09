using BarberShop.Data;
using BarberShop.Models;
using BarberShop.Services.Profile;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarberShop.Tests;

public class ProfileMessageTests
{
    private static BarberShopDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BarberShopDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BarberShopDbContext(options);
    }

    [Fact]
    public async Task SendContactMessageAsync_CreatesUnreadMessage()
    {
        using var context = CreateContext();
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString("N"), UserName = "jane", Email = "jane@shop.com", FirstName = "Jane", LastName = "Doe" };
        var userManager = TestUserManagerFactory.Create(user);
        var service = new ProfileService(userManager, context);

        var result = await service.SendContactMessageAsync(user.Id, new ContactMessageViewModel
        {
            Subject = "Need help",
            Message = "Please call me back."
        });

        var saved = await context.ContactMessages.SingleAsync();

        Assert.True(result.Success);
        Assert.Equal("Message sent successfully", result.Message);
        Assert.False(saved.IsRead);
        Assert.Equal(user.Id, saved.UserId);
        Assert.Equal("Need help", saved.Subject);
    }

    [Fact]
    public async Task GetContactMessagesAsync_ReturnsOnlyUserMessages()
    {
        using var context = CreateContext();
        context.ContactMessages.AddRange(
            new ContactMessage { MessageId = 1, UserId = "user-1", Subject = "One", Message = "First", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new ContactMessage { MessageId = 2, UserId = "user-1", Subject = "Two", Message = "Second", IsRead = true, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new ContactMessage { MessageId = 3, UserId = "user-2", Subject = "Other", Message = "Third", IsRead = false, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ProfileService(TestUserManagerFactory.Create(), context);
        var messages = await service.GetContactMessagesAsync("user-1");

        Assert.Equal(2, messages.Count);
        Assert.All(messages, message => Assert.Equal("user-1", message.UserId));
    }

    [Fact]
    public async Task GetAllContactMessagesAsync_CanFilterUnreadOnly()
    {
        using var context = CreateContext();
        context.ContactMessages.AddRange(
            new ContactMessage { MessageId = 1, UserId = "user-1", Subject = "One", Message = "First", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new ContactMessage { MessageId = 2, UserId = "user-1", Subject = "Two", Message = "Second", IsRead = true, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new ContactMessage { MessageId = 3, UserId = "user-2", Subject = "Other", Message = "Third", IsRead = false, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ProfileService(TestUserManagerFactory.Create(), context);
        var all = await service.GetAllContactMessagesAsync();
        var unread = await service.GetAllContactMessagesAsync(unreadOnly: true);

        Assert.Equal(3, all.Count);
        Assert.Equal(2, unread.Count);
        Assert.All(unread, message => Assert.False(message.IsRead));
    }

    [Fact]
    public async Task MarkMessageAsReadAsync_And_DeleteMessageAsync_Work()
    {
        using var context = CreateContext();
        context.ContactMessages.Add(new ContactMessage
        {
            MessageId = 1,
            UserId = "user-1",
            Subject = "One",
            Message = "First",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new ProfileService(TestUserManagerFactory.Create(), context);

        var marked = await service.MarkMessageAsReadAsync(1);
        var deleted = await service.DeleteMessageAsync(1);

        Assert.True(marked);
        Assert.True(deleted);
        Assert.Empty(await context.ContactMessages.ToListAsync());
    }
}
