using Microsoft.EntityFrameworkCore;

using Catalogs.Domain.Entities;
using Catalogs.Infrastructure.Persistence;
using Catalogs.Infrastructure.Services;

namespace Catalogs.Infrastructure.Repositories;

public class CatalogsRepository : ICatalogsRepository
{
    private readonly CatalogsDbContext _context;

    public CatalogsRepository(CatalogsDbContext context)
    {
        _context = context;
    }

    // Service methods
    public async Task<Service> GetServiceById(Guid id)
        => await _context.Services
            .Include(s => s.PricingOptions)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<Service>> GetAllServices()
        => await _context.Services
            .Include(s => s.PricingOptions)
            .ToListAsync();

    public async Task<Service> AddService(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return service;
    }

    public async Task<Service> UpdateService(Service service)
    {
        var existing = await _context.Services.FirstOrDefaultAsync(x => x.Id == service.Id);
        if (existing == null) return service;

        existing.Name        = service.Name;
        existing.Description = service.Description;
        existing.IsActive    = service.IsActive;
        existing.Icon        = service.Icon;
        existing.ThemeIcon   = service.ThemeIcon;
        // Price and UoM are read-only; managed via PricingOptions

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteService(Guid id)
    {
        var service = await _context.Services.FirstOrDefaultAsync(x => x.Id == id);
        if (service == null) return false;

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return true;
    }

    // PricingOption methods
    public async Task<IEnumerable<ServicePricingOption>> GetPricingOptionsByServiceId(Guid serviceId)
        => await _context.ServicePricingOptions
            .Where(x => x.ServiceId == serviceId)
            .OrderBy(x => x.OptionName)
            .ToListAsync();

    public async Task<ServicePricingOption?> GetPricingOptionById(Guid optionId)
        => await _context.ServicePricingOptions.FirstOrDefaultAsync(x => x.Id == optionId);

    public async Task<ServicePricingOption> AddPricingOption(ServicePricingOption option)
    {
        _context.ServicePricingOptions.Add(option);
        await _context.SaveChangesAsync();
        return option;
    }

    public async Task<ServicePricingOption> UpdatePricingOption(ServicePricingOption option)
    {
        _context.ServicePricingOptions.Update(option);
        await _context.SaveChangesAsync();
        return option;
    }

    public async Task<bool> DeletePricingOption(Guid optionId)
    {
        var option = await _context.ServicePricingOptions.FirstOrDefaultAsync(x => x.Id == optionId);
        if (option == null) return false;

        _context.ServicePricingOptions.Remove(option);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetActivePricingOptionsCountByServiceId(Guid serviceId)
        => await _context.ServicePricingOptions
            .CountAsync(x => x.ServiceId == serviceId && x.IsActive);

    // Courier methods
    public async Task<Courier> GetCourierById(Guid id)
        => await _context.Couriers.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Courier> GetCourierByAuthUserId(string authUserId)
        => await _context.Couriers.FirstOrDefaultAsync(x => x.AuthUserId == authUserId);

    public async Task<IEnumerable<Courier>> GetAllCouriers()
        => await _context.Couriers.ToListAsync();

    public async Task<Courier> AddCourier(Courier courier)
    {
        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync();
        return courier;
    }

    public async Task<Courier> UpdateCourier(Courier courier)
    {
        _context.Couriers.Update(courier);
        await _context.SaveChangesAsync();
        return courier;
    }

    public async Task<bool> DeleteCourier(Guid id)
    {
        var courier = await _context.Couriers.FirstOrDefaultAsync(x => x.Id == id);
        if (courier == null) return false;

        _context.Couriers.Remove(courier);
        await _context.SaveChangesAsync();
        return true;
    }

    // Coupon methods
    public async Task<Coupon?> GetCouponById(Guid id)
        => await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Coupon?> GetCouponByCode(string code)
        => await _context.Coupons.FirstOrDefaultAsync(x => x.Code == code);

    public async Task<IEnumerable<Coupon>> GetAllCoupons()
        => await _context.Coupons.OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<Coupon> AddCoupon(Coupon coupon)
    {
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        return coupon;
    }

    public async Task<Coupon> UpdateCoupon(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
        return coupon;
    }

    public async Task<bool> DeleteCoupon(Guid id)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id);
        if (coupon == null) return false;

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
        return true;
    }
}