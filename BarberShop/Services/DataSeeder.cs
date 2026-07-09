using BarberShop.Data;
using BarberShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Services;

public class DataSeeder(BarberShopDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync()
    {
        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("Customer"))
        {
            await roleManager.CreateAsync(new IdentityRole("Customer"));
        }

        // Seed Dummy Accounts with Confirmed Emails
        await SeedDummyAccountsAsync();

        // Seed Services
        if (!await db.Services.AnyAsync())
        {
            var services = new List<Service>
            {
                new Service
                {
                    Name = "Classic Haircut",
                    Description = "Traditional haircut with scissor work",
                    BasePrice = 25.00m,
                    BaseDuration = 30,
                    IsActive = true
                },
                new Service
                {
                    Name = "Fade Haircut",
                    Description = "Modern fade cut with clean lines",
                    BasePrice = 30.00m,
                    BaseDuration = 35,
                    IsActive = true
                },
                new Service
                {
                    Name = "Premium Haircut",
                    Description = "Premium cut with styling and finishing",
                    BasePrice = 40.00m,
                    BaseDuration = 45,
                    IsActive = true
                },
                new Service
                {
                    Name = "Beard Trim",
                    Description = "Professional beard trimming and shaping",
                    BasePrice = 20.00m,
                    BaseDuration = 20,
                    IsActive = true
                },
                new Service
                {
                    Name = "Hot Shave",
                    Description = "Classic hot lather shave with straight razor",
                    BasePrice = 35.00m,
                    BaseDuration = 30,
                    IsActive = true
                },
                new Service
                {
                    Name = "Kids Haircut",
                    Description = "Haircut tailored for children",
                    BasePrice = 15.00m,
                    BaseDuration = 20,
                    IsActive = true
                },
                new Service
                {
                    Name = "Hair & Beard Combo",
                    Description = "Haircut combined with beard trim and shave",
                    BasePrice = 60.00m,
                    BaseDuration = 60,
                    IsActive = true
                }
            };

            await db.Services.AddRangeAsync(services);
            await db.SaveChangesAsync();
        }

        // Seed Barbers
        if (!await db.Barbers.AnyAsync())
        {
            var barbers = new List<Barber>
            {
                new Barber
                {
                    FirstName = "Andrei",
                    LastName = "Barberescu",
                    Phone = "+40722123456",
                    Email = "andrei@barbershop.com",
                    HireDate = DateTime.Now.AddYears(-5),
                    IsActive = true,
                    PhotoFileName = "Andrei Barberescu.jpg"
                },
                new Barber
                {
                    FirstName = "Dorel",
                    LastName = "Conturistul",
                    Phone = "+40722123457",
                    Email = "dorel@barbershop.com",
                    HireDate = DateTime.Now.AddYears(-3),
                    IsActive = true,
                    PhotoFileName = "Dorel Conturistul.jpg"
                },
                new Barber
                {
                    FirstName = "Johny",
                    LastName = "McBarber",
                    Phone = "+40722123458",
                    Email = "johny@barbershop.com",
                    HireDate = DateTime.Now.AddYears(-2),
                    IsActive = true,
                    PhotoFileName = "JohnyMcBarber.jpg"
                },
                new Barber
                {
                    FirstName = "Mihai",
                    LastName = "Brici",
                    Phone = "+40722123459",
                    Email = "mihai@barbershop.com",
                    HireDate = DateTime.Now.AddYears(-4),
                    IsActive = true,
                    PhotoFileName = "Mihai Brici.jpg"
                },
                new Barber
                {
                    FirstName = "Stephany",
                    LastName = "Shearing",
                    Phone = "+40722123460",
                    Email = "stephany@barbershop.com",
                    HireDate = DateTime.Now.AddYears(-1),
                    IsActive = true,
                    PhotoFileName = "Stephany Shearing.jpg"
                }
            };

            await db.Barbers.AddRangeAsync(barbers);
            await db.SaveChangesAsync();
        }

        // Seed Products
        if (!await db.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Beard Oil",
                    Description = "Premium beard oil for softness and shine",
                    Category = "Beard Care",
                    Price = 19.99m,
                    StockQuantity = 50,
                    IsActive = true
                },
                new Product
                {
                    Name = "Hair Pomade",
                    Description = "Strong hold pomade with matte finish",
                    Category = "Hair Styling",
                    Price = 15.99m,
                    StockQuantity = 75,
                    IsActive = true
                },
                new Product
                {
                    Name = "Beard Balm",
                    Description = "Natural beard balm for styling and conditioning",
                    Category = "Beard Care",
                    Price = 17.99m,
                    StockQuantity = 60,
                    IsActive = true
                },
                new Product
                {
                    Name = "Aftershave Cream",
                    Description = "Soothing aftershave cream",
                    Category = "Shaving",
                    Price = 12.99m,
                    StockQuantity = 100,
                    IsActive = true
                },
                new Product
                {
                    Name = "Shampoo - Premium",
                    Description = "High-quality barber shampoo",
                    Category = "Hair Care",
                    Price = 14.99m,
                    StockQuantity = 80,
                    IsActive = true
                },
                new Product
                {
                    Name = "Conditioner",
                    Description = "Moisturizing conditioner for all hair types",
                    Category = "Hair Care",
                    Price = 14.99m,
                    StockQuantity = 70,
                    IsActive = true
                },
                new Product
                {
                    Name = "Beard Brush",
                    Description = "Natural bristle beard brush",
                    Category = "Accessories",
                    Price = 24.99m,
                    StockQuantity = 40,
                    IsActive = true
                },
                new Product
                {
                    Name = "Hair Wax",
                    Description = "Flexible hold hair wax",
                    Category = "Hair Styling",
                    Price = 13.99m,
                    StockQuantity = 65,
                    IsActive = true
                },
                new Product
                {
                    Name = "Cologne",
                    Description = "Fresh, masculine cologne",
                    Category = "Fragrance",
                    Price = 49.99m,
                    StockQuantity = 30,
                    IsActive = true
                },
                new Product
                {
                    Name = "Styling Clay",
                    Description = "Textured styling clay for modern looks",
                    Category = "Hair Styling",
                    Price = 16.99m,
                    StockQuantity = 55,
                    IsActive = true
                }
            };

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
        }
    }

    // Seed Dummy User Accounts with Confirmed Emails
    private async Task SeedDummyAccountsAsync()
    {
        var dummyAccounts = new List<(string Email, string Name, string Password, string Role, string MembershipTier)>
        {
            ("admin@barbershop.com", "Admin User", "Admin@123456", "Admin", null),
            ("john.customer@barbershop.com", "John Customer", "Customer@123456", "Customer", "Bronze"),
            ("jane.premium@barbershop.com", "Jane Smith", "Premium@123456", "Customer", "Gold"),
            ("mike.silver@barbershop.com", "Mike Johnson", "Member@123456", "Customer", "Silver"),
            ("test@barbershop.com", "Test User", "Test@123456", "Customer", null),
        };

        foreach (var (email, name, password, role, membership) in dummyAccounts)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = name.Split(' ')[0],
                    LastName = name.Contains(' ') ? string.Join(" ", name.Split(' ').Skip(1)) : "User",
                    EmailConfirmed = true,  // Pre-confirmed for testing
                    MembershipTier = membership,
                    MembershipExpires = membership != null ? DateTime.UtcNow.AddMonths(1) : null,
                    PhoneNumber = "+40722123999",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    // Assign role
                    await userManager.AddToRoleAsync(user, role);
                    Console.WriteLine($"✓ Created {role} account: {email}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to create {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
