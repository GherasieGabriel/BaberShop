using BarberShop.Data;
using BarberShop.Models;
using Microsoft.AspNetCore.Identity;

namespace BarberShop.Services.Image;

public class ImageService(
    UserManager<ApplicationUser> userManager,
    BarberShopDbContext dbContext) : IImageService
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/gif" };
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    public async Task<(bool Success, string Message)> UploadProfileImageAsync(string userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "No file was uploaded");
        }

        // Validate file size
        if (file.Length > MaxFileSize)
        {
            return (false, $"File size exceeds maximum allowed size of 5MB");
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
        {
            return (false, "Only JPG, PNG, and GIF files are allowed");
        }

        // Validate MIME type
        if (!_allowedMimeTypes.Contains(file.ContentType))
        {
            return (false, "Invalid file type. Only image files are allowed");
        }

        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Read file into byte array
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                user.ProfileImage = memoryStream.ToArray();
            }

            // Update user with new image
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return (false, "Failed to upload image");
            }

            return (true, "Profile image uploaded successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error uploading image: {ex.Message}");
        }
    }

    public async Task<byte[]?> GetProfileImageAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.ProfileImage;
    }

    public async Task<bool> DeleteProfileImageAsync(string userId)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.ProfileImage = null;
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch
        {
            return false;
        }
    }

    public string GetImageMimeType(byte[]? imageData)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return "image/png";
        }

        // Check PNG signature
        if (imageData.Length >= 4 && imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
        {
            return "image/png";
        }

        // Check JPEG signature
        if (imageData.Length >= 2 && imageData[0] == 0xFF && imageData[1] == 0xD8)
        {
            return "image/jpeg";
        }

        // Check GIF signature
        if (imageData.Length >= 3 && imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
        {
            return "image/gif";
        }

        return "image/png"; // Default
    }
}
