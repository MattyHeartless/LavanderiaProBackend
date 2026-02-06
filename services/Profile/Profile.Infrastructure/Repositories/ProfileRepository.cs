


using Microsoft.EntityFrameworkCore;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Infrastructure.Persistence;

namespace Profile.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly ProfileDbContext _context;

    public ProfileRepository(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAddress>> GetAddressesAsync(Guid userId)
        => await _context.UserAddresses
            .Where(x => x.UserId == userId)
            .ToListAsync();

    public async Task AddAddressAsync(UserAddress address)
    {
        _context.UserAddresses.Add(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(int addressId, Guid userId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId);

        if (address == null) return;

        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAddressAsync(UserAddress address)
    {
        _context.UserAddresses.Update(address);
        await _context.SaveChangesAsync();
    }


    // Payment methods
    public async Task<IEnumerable<UserPaymentMethod>> GetPaymentMethodsAsync(Guid userId)
        => await _context.UserPaymentMethods
            .Where(x => x.UserId == userId)
            .ToListAsync();

    public async Task AddPaymentMethodAsync(UserPaymentMethod method)
    {
        _context.UserPaymentMethods.Add(method);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePaymentMethodAsync(int methodId, Guid userId)
    {
        var method = await _context.UserPaymentMethods
            .FirstOrDefaultAsync(x => x.Id == methodId && x.UserId == userId);

        if (method == null) return;

        _context.UserPaymentMethods.Remove(method);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePaymentMethodAsync(UserPaymentMethod method)
    {
        _context.UserPaymentMethods.Update(method);
        await _context.SaveChangesAsync();
    }
    
}
