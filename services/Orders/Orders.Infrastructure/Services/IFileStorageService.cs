using Microsoft.AspNetCore.Http;

namespace Orders.Infrastructure.Services;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveOrderEvidenceAsync(IFormFile file);
}

public class FileStorageResult
{
    public string RelativePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
