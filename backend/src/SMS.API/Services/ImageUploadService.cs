using System.ComponentModel.DataAnnotations;

namespace SMS.API.Services;

/// <summary>
/// Service for handling file uploads (images for students and teachers)
/// </summary>
public interface IImageUploadService
{
    Task<string> UploadImageAsync(IFormFile file, string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string imagePath, CancellationToken cancellationToken = default);
    bool IsValidImageFile(IFormFile file);
}

public class ImageUploadService : IImageUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidImageFile(file))
            {
                throw new ValidationException("Invalid image file. Please upload a valid image (jpg, jpeg, png, gif, webp) under 5MB.");
            }

            // Create uploads directory structure
            var uploadsDir = Path.Combine(_environment.WebRootPath ?? "./wwwroot", "uploads", entityType.ToLower());
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{entityId}_{DateTime.UtcNow.Ticks}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Return relative path for storing in database
            var relativePath = $"/uploads/{entityType.ToLower()}/{fileName}";
            _logger.LogInformation("Image uploaded successfully for {EntityType} {EntityId}: {RelativePath}", entityType, entityId, relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return false;

            var filePath = Path.Combine(_environment.WebRootPath ?? "./wwwroot", imagePath.TrimStart('/'));

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Image file not found: {FilePath}", filePath);
                return false;
            }

            File.Delete(filePath);
            _logger.LogInformation("Image deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImagePath}", imagePath);
            return false;
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > _maxFileSize)
            return false;

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
            return false;

        return true;
    }
}
