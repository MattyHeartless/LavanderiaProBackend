
namespace Catalogs.Domain.Entities
{
    public class Courier
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Vehicle { get; set; }
        public string Address { get; set; }
        public string Neighborhood { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public Boolean IsActive { get; set; }
    }
}

