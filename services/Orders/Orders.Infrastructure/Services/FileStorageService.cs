using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Orders.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private const long EvidenceMaxBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly string _imagesRootPath;

    public FileStorageService(IConfiguration configuration)
    {
        _imagesRootPath = configuration["Storage:ImagesRootPath"]
            ?? @"C:\Users\Jair\Documents\My Web Sites\LaundrAppBackend\LavanderiaProBackend\services\Orders\images";
    }

    public async Task<FileStorageResult> SaveOrderEvidenceAsync(IFormFile file)
    {
        ValidateFile(file, EvidenceMaxBytes);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativeFolder = Path.Combine("orders", "evidences");

        var folderPath = Path.Combine(_imagesRootPath, relativeFolder);
        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/images/{relativeFolder.Replace(Path.DirectorySeparatorChar, '/').Replace('\\', '/')}/{fileName}";

        return new FileStorageResult
        {
            RelativePath = relativePath,
            PublicUrl = relativePath,
            MimeType = file.ContentType,
            SizeBytes = file.Length
        };
    }

    private static void ValidateFile(IFormFile? file, long maxSizeBytes)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("File is required.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid file extension. Allowed: .jpg, .jpeg, .png, .webp");

        if (file.Length > maxSizeBytes)
            throw new InvalidOperationException($"File size exceeded. Max allowed: {maxSizeBytes / (1024 * 1024)} MB");
    }
}
