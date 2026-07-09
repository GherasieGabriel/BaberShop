using BarberShop.Models;
using BarberShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;

namespace BarberShop.Controllers;

public class HomeController(IServiceCatalogService serviceCatalogService, UserManager<ApplicationUser> userManager) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Prices()
    {
        var services = await serviceCatalogService.GetAllAsync();
        return View(services);
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Gallery()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Membership list page
    public IActionResult Membership()
    {
        var model = new MembershipViewModel
        {
            Tiers = new List<MembershipTier>
            {
                new MembershipTier("Bronze", 10, "Affordable membership with basic perks."),
                new MembershipTier("Silver", 20, "Popular choice: better discounts and priority booking."),
                new MembershipTier("Gold", 30, "Premium benefits, highest discounts and perks.")
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Subscribe(string tier)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // require login to subscribe
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Membership", "Home") });
        }

        // redirect to payment page
        return RedirectToAction("Payment", new { tier });
    }

    [HttpGet]
    public IActionResult Payment(string tier)
    {
        if (string.IsNullOrWhiteSpace(tier))
            return RedirectToAction("Membership");

        var fee = tier switch
        {
            "Bronze" => 50m,
            "Silver" => 80m,
            "Gold" => 120m,
            _ => 50m
        };

        var model = new MembershipPaymentViewModel { Tier = tier, Fee = fee };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(string tier)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Membership", "Home") });
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // simulate payment success
        var expires = DateTime.UtcNow.AddYears(1);

        // persist membership to user record
        user.MembershipTier = tier;
        user.MembershipExpires = expires;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Failed to save membership to user profile.";
            return RedirectToAction("Membership");
        }

        // remove existing membership claims and add fresh ones
        var claims = await userManager.GetClaimsAsync(user);
        var existingMembership = claims.FirstOrDefault(c => c.Type == "membership");
        if (existingMembership != null)
            await userManager.RemoveClaimAsync(user, existingMembership);

        var existingExpire = claims.FirstOrDefault(c => c.Type == "membershipExpires");
        if (existingExpire != null)
            await userManager.RemoveClaimAsync(user, existingExpire);

        await userManager.AddClaimAsync(user, new Claim("membership", tier));
        await userManager.AddClaimAsync(user, new Claim("membershipExpires", expires.ToString("o")));

        TempData["SuccessMessage"] = "Membership activated successfully.";

        return RedirectToAction("MembershipConfirmation");
    }

    [HttpGet]
    public async Task<IActionResult> MembershipConfirmation()
    {
        // show confirmation details – user claims include membership
        return View();
    }

    // Added: SpecialOffers page - selects one active service and applies a discount
    public async Task<IActionResult> SpecialOffers()
    {
        var services = await serviceCatalogService.GetAllAsync();
        var service = services?.FirstOrDefault(s => s.IsActive) ?? services?.FirstOrDefault();

        if (service == null)
        {
            return View(new SpecialOfferViewModel());
        }

        var discountPercent = 20; // default discount for special offers
        var discountedPrice = service.BasePrice * (1 - discountPercent / 100m);

        var model = new SpecialOfferViewModel
        {
            Service = service,
            DiscountPercent = discountPercent,
            DiscountedPrice = discountedPrice
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
