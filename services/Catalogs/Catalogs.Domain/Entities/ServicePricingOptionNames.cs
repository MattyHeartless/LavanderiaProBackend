namespace Catalogs.Domain.Entities
{
    public static class ServicePricingOptionNames
    {
        public const string PorKilo      = "Por kilo";
        public const string PorPieza     = "Por pieza";
        public const string PorDocena    = "Por docena";
        public const string BultoPequeño = "Bulto pequeño";
        public const string BultoMediano = "Bulto mediano";
        public const string BultoGrande  = "Bulto grande";
        public const string BultoJumbo   = "Bulto jumbo";

        public static readonly IReadOnlyDictionary<string, string> RequiredUoM =
            new Dictionary<string, string>
            {
                [PorKilo]      = "KG",
                [PorPieza]     = "PZ",
                [PorDocena]    = "DOC",
                [BultoPequeño] = "BULTO",
                [BultoMediano] = "BULTO",
                [BultoGrande]  = "BULTO",
                [BultoJumbo]   = "BULTO",
            };
    }
}
