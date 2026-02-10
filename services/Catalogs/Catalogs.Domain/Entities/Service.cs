namespace Catalogs.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string UoM { get; set; }
        public Boolean IsActive { get; set; }
        public string Icon { get; set; }
        public string ThemeIcon { get; set; }
    }
}