namespace BarberShop.Services.Image;

public interface IImageService
{
    Task<(bool Success, string Message)> UploadProfileImageAsync(string userId, IFormFile file);
    Task<byte[]?> GetProfileImageAsync(string userId);
    Task<bool> DeleteProfileImageAsync(string userId);
    string GetImageMimeType(byte[]? imageData);
}
