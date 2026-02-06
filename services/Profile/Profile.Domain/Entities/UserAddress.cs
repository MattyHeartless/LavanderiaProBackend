namespace Profile.Domain.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Neighbourhood { get; set; } 
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;  
        public DateTime? UpdatedAt { get; set; }
    }
}   