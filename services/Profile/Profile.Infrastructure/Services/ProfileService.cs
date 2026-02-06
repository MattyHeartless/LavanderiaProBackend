using Profile.Application.Interfaces;
using Profile.Domain.Entities;

namespace Profile.Infrastructure.Services;

public class ProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    // Addresses
    public async Task<IEnumerable<UserAddress>> GetAddressesAsync(Guid userId)
    {
        return await _profileRepository.GetAddressesAsync(userId);
    }

    public async Task AddAddressAsync(UserAddress address)
    {
        await _profileRepository.AddAddressAsync(address);
    }

    public async Task DeleteAddressAsync(int addressId, Guid userId)
    {
        await _profileRepository.DeleteAddressAsync(addressId, userId);
    }

    // Payment methods
    public async Task<IEnumerable<UserPaymentMethod>> GetPaymentMethodsAsync(Guid userId)
    {
        return await _profileRepository.GetPaymentMethodsAsync(userId);
    }

    public async Task AddPaymentMethodAsync(UserPaymentMethod method)
    {
        await _profileRepository.AddPaymentMethodAsync(method);
    }

    public async Task DeletePaymentMethodAsync(int methodId, Guid userId)
    {
        await _profileRepository.DeletePaymentMethodAsync(methodId, userId);
    }
}