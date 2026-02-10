public class PaymentMethod
{
    public Guid Id { get; set; }
    public string CardHolderName { get; set; } = default!;
    public string CardNumber { get; set;}
    public string ExpirationDate { get; set; }
    public string CVV { get; set; }

    public PaymentMethod() { } // EF

    public PaymentMethod(Guid id, string cardHolderName, string cardNumber, string expirationDate, string cvv)
    {
        Id = id;
        CardHolderName = cardHolderName;
        CardNumber = cardNumber;
        ExpirationDate = expirationDate;
        CVV = cvv;
    }
}