using Microsoft.AspNetCore.Http;

namespace Catalogs.API.DTOs;

public class UploadProfileImageRequest
{
    public IFormFile? File { get; set; }
}

public class UploadProfileImageResponse
{
    public string Message { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
}
