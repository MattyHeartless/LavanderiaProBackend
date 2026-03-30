namespace Orders.Domain.Entities;

public class DeliveryMode
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int EtaHours { get; set; }
    public decimal SurchargeAmount { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}