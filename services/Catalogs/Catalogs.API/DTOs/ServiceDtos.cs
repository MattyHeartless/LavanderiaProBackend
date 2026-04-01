namespace Catalogs.API.DTOs;

public class UpdateServiceRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string UoM { get; set; } = default!;
    public bool IsActive { get; set; }
    public string Icon { get; set; } = default!;
    public string ThemeIcon { get; set; } = default!;
    public List<UpdateServicePricingOptionRequest>? PricingOptions { get; set; }
}

public class UpdateServicePricingOptionRequest
{
    public string? Id { get; set; }
    public string? ServiceId { get; set; }
    public string OptionName { get; set; } = default!;
    public decimal Price { get; set; }
    public string UoM { get; set; } = default!;
    public bool IsActive { get; set; }
}

public class CreatePricingOptionRequest
{
    public string OptionName { get; set; } = default!;
    public decimal Price { get; set; }
    public string UoM { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}

public class UpdatePricingOptionRequest
{
    public string OptionName { get; set; } = default!;
    public decimal Price { get; set; }
    public string UoM { get; set; } = default!;
    public bool IsActive { get; set; }
}
