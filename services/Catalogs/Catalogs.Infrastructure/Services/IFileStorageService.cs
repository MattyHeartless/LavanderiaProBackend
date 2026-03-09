using Microsoft.AspNetCore.Http;

namespace Catalogs.Infrastructure.Services;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveCourierProfileImageAsync(IFormFile file);
    Task DeleteByRelativePathAsync(string? relativePath);
}

public class FileStorageResult
{
    public string RelativePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
