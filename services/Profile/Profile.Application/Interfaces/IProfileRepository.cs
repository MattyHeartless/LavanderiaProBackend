using Profile.Domain.Entities;

namespace Profile.Application.Interfaces;

public interface IProfileRepository
{
    // Addresses
    Task<IEnumerable<UserAddress>> GetAddressesAsync(Guid userId);
    Task AddAddressAsync(UserAddress address);
    Task DeleteAddressAsync(int addressId, Guid userId);
    Task UpdateAddressAsync(UserAddress address);

    // Payment methods
    Task<IEnumerable<UserPaymentMethod>> GetPaymentMethodsAsync(Guid userId);
    Task AddPaymentMethodAsync(UserPaymentMethod method);
    Task DeletePaymentMethodAsync(int methodId, Guid userId);

    Task UpdatePaymentMethodAsync(UserPaymentMethod method);
}
