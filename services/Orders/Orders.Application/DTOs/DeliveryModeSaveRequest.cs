namespace Orders.Application.DTOs;

public sealed class DeliveryModeSaveRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? EtaHours { get; set; }
    public decimal? SurchargeAmount { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
