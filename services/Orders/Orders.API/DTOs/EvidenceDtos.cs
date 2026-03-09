using Microsoft.AspNetCore.Http;

namespace Orders.API.DTOs;

public class UploadEvidenceRequest
{
    public IFormFile? File { get; set; }
    public string? Note { get; set; }
    public Guid? CourierId { get; set; }
}

public class OrderEvidenceResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadEvidenceResponse
{
    public string Message { get; set; } = string.Empty;
    public OrderEvidenceResponse Evidence { get; set; } = new();
}
