namespace Catalogs.Domain.Entities
{
    public static class ServicePricingOptionRules
    {
        public static string? Validate(string? optionName, decimal price, string? uoM)
        {
            if (string.IsNullOrWhiteSpace(optionName))
                return "OptionName is required.";

            if (!ServicePricingOptionNames.RequiredUoM.TryGetValue(optionName, out var expectedUoM))
                return $"Unsupported option name '{optionName}'. Valid values: {string.Join(", ", ServicePricingOptionNames.RequiredUoM.Keys)}.";

            if (string.IsNullOrWhiteSpace(uoM) || uoM != expectedUoM)
                return $"UoM for '{optionName}' must be '{expectedUoM}'.";

            if (price <= 0)
                return "Price must be greater than zero.";

            return null;
        }
    }
}
