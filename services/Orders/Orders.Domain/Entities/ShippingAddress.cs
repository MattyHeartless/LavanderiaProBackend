public class ShippingAddress
{
    public string Title { get; set; }
    public string Street { get; private set; }
    public string Neighbourhood { get; set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    

    private ShippingAddress() { } // EF

    public ShippingAddress(
        string title,
        string street,
        string neighbourhood,
        string city,
        string state,
        string zipCode,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        Title = title;
        Street = street;
        Neighbourhood = neighbourhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Latitude = latitude;
        Longitude = longitude;
    }
}
