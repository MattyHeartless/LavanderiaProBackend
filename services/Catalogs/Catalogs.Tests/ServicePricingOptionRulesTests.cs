using Catalogs.Domain.Entities;

namespace Catalogs.Tests;

public class ServicePricingOptionRulesTests
{
    [Fact]
    public void Validate_ReturnsError_WhenOptionNameIsEmpty()
    {
        var result = ServicePricingOptionRules.Validate(string.Empty, 20m, "KG");
        Assert.NotNull(result);
        Assert.Contains("OptionName is required", result);
    }

    [Fact]
    public void Validate_ReturnsError_WhenOptionNameIsNull()
    {
        var result = ServicePricingOptionRules.Validate(null, 20m, "KG");
        Assert.NotNull(result);
        Assert.Contains("OptionName is required", result);
    }

    [Fact]
    public void Validate_ReturnsError_WhenOptionNameIsUnsupported()
    {
        var result = ServicePricingOptionRules.Validate("Opcion invalida", 20m, "KG");
        Assert.NotNull(result);
        Assert.Contains("Unsupported option name", result);
    }

    [Fact]
    public void Validate_ReturnsError_WhenUoMDoesNotMatchOptionName()
    {
        var result = ServicePricingOptionRules.Validate("Por kilo", 20m, "PZ");
        Assert.NotNull(result);
        Assert.Contains("'KG'", result);
    }

    [Fact]
    public void Validate_ReturnsError_WhenPriceIsZero()
    {
        var result = ServicePricingOptionRules.Validate("Por kilo", 0m, "KG");
        Assert.NotNull(result);
        Assert.Contains("greater than zero", result);
    }

    [Fact]
    public void Validate_ReturnsError_WhenPriceIsNegative()
    {
        var result = ServicePricingOptionRules.Validate("Por kilo", -5m, "KG");
        Assert.NotNull(result);
        Assert.Contains("greater than zero", result);
    }

    [Fact]
    public void Validate_ReturnsNull_ForValidPorKilo()
    {
        var result = ServicePricingOptionRules.Validate("Por kilo", 20m, "KG");
        Assert.Null(result);
    }

    [Fact]
    public void Validate_ReturnsNull_ForValidPorPieza()
    {
        var result = ServicePricingOptionRules.Validate("Por pieza", 15m, "PZ");
        Assert.Null(result);
    }

    [Fact]
    public void Validate_ReturnsNull_ForValidPorDocena()
    {
        var result = ServicePricingOptionRules.Validate("Por docena", 50m, "DOC");
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Bulto pequeño")]
    [InlineData("Bulto mediano")]
    [InlineData("Bulto grande")]
    [InlineData("Bulto jumbo")]
    public void Validate_ReturnsNull_ForAllBultoOptions(string optionName)
    {
        var result = ServicePricingOptionRules.Validate(optionName, 100m, "BULTO");
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Bulto pequeño", "KG")]
    [InlineData("Bulto mediano", "PZ")]
    [InlineData("Bulto grande",  "DOC")]
    public void Validate_ReturnsError_WhenBultoOptionHasWrongUoM(string optionName, string wrongUoM)
    {
        var result = ServicePricingOptionRules.Validate(optionName, 100m, wrongUoM);
        Assert.NotNull(result);
        Assert.Contains("'BULTO'", result);
    }
}
