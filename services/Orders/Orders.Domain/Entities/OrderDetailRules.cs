namespace Orders.Domain.Entities;

public static class OrderDetailRules
{
    /// <summary>
    /// Validates an OrderDetail, with special rules for Bulto options.
    /// If pricingOptionName contains "Bulto", requires both ColoredClothQuantity and BlackClothQuantity to be provided.
    /// Otherwise, requires Quantity to be > 0.
    /// </summary>
    public static string? Validate(OrderDetail detail)
    {
        if (string.IsNullOrWhiteSpace(detail.ServiceId))
            return "ServiceId is required.";

        if (string.IsNullOrWhiteSpace(detail.ServiceName))
            return "ServiceName is required.";

        if (detail.ServicePrice <= 0)
            return "ServicePrice must be greater than zero.";

        if (string.IsNullOrWhiteSpace(detail.UoM))
            return "UoM is required.";

        bool isBulto = !string.IsNullOrWhiteSpace(detail.PricingOptionName) 
            && detail.PricingOptionName.Contains("Bulto", StringComparison.OrdinalIgnoreCase);

        if (isBulto)
        {
            if (!detail.ColoredClothQuantity.HasValue || detail.ColoredClothQuantity < 0)
                return "ColoredClothQuantity is required and must be >= 0 for Bulto options.";

            if (!detail.BlackClothQuantity.HasValue || detail.BlackClothQuantity < 0)
                return "BlackClothQuantity is required and must be >= 0 for Bulto options.";

            if (detail.ColoredClothQuantity.Value + detail.BlackClothQuantity.Value <= 0)
                return "Total cloth quantity (ColoredClothQuantity + BlackClothQuantity) must be > 0 for Bulto options.";
        }
        else
        {
            if (detail.Quantity <= 0)
                return "Quantity must be greater than zero.";
        }

        return null;
    }
}
