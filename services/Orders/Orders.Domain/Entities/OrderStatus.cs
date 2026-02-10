namespace Orders.Domain.Entities;

public enum OrderStatus
{
    Created = 1,
    Paid = 2,
    Recollecting = 3,
    Processing = 4,
    Delivering = 5,
    Completed = 6,
    Cancelled = 7
}
