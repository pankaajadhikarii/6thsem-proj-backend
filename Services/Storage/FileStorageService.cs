using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Bizkit_backend.Services.Storage;

public sealed class FileStorageService(
    IWebHostEnvironment webHostEnvironment) : IFileStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB

    public async Task<(bool Succeeded, string? FilePath, string? ErrorMessage)> SaveFileAsync(
        IFormFile file,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return (false, null, "No file uploaded.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, null, "File size must not exceed 2MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (Array.IndexOf(AllowedExtensions, extension) < 0)
        {
            return (false, null, "Only .jpg, .jpeg, and .png image files are allowed.");
        }

        var webRootPath = webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/uploads/{folderName}/{uniqueFileName}";

        return (true, relativePath, null);
    }

    public void DeleteFile(string? relativeFilePath)
    {
        if (string.IsNullOrWhiteSpace(relativeFilePath))
        {
            return;
        }

        var webRootPath = webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var relative = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRootPath, relative);

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                // Ignore file deletion errors
            }
        }
    }
}
