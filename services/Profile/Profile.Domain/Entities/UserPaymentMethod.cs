namespace Profile.Domain.Entities;

    public class UserPaymentMethod
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string CardHolderName { get; set; }
    public string CardNumber { get; set; }
    public string ExpirationDate { get; set; }
    public string CVV { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;  
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}

