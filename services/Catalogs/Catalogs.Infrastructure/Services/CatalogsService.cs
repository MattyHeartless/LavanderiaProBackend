
using Catalogs.Domain.Entities;

namespace Catalogs.Infrastructure.Services;

public class CatalogsService
{
    private readonly ICatalogsRepository _catalogsRepository;

    public CatalogsService(ICatalogsRepository catalogsRepository)
    {
        _catalogsRepository = catalogsRepository;
    }

    // Service methods
    public async Task<Service> GetServiceByIdAsync(Guid id)
    {
        return await _catalogsRepository.GetServiceById(id);
    }

    public async Task<IEnumerable<Service>> GetAllServicesAsync()
    {
        return await _catalogsRepository.GetAllServices();
    }

    public async Task<Service> AddServiceAsync(Service service)
    {
        return await _catalogsRepository.AddService(service);
    }

    public async Task<Service> UpdateServiceAsync(Service service)
    {
        return await _catalogsRepository.UpdateService(service);
    }

    public async Task<bool> DeleteServiceAsync(Guid id)
    {
        return await _catalogsRepository.DeleteService(id);
    }

    // Courier methods
    public async Task<Courier> GetCourierByIdAsync(Guid id)
    {
        return await _catalogsRepository.GetCourierById(id);
    }

    public async Task<Courier> GetCourierByAuthUserIdAsync(string authUserId)
    {
        return await _catalogsRepository.GetCourierByAuthUserId(authUserId);
    }

    public async Task<IEnumerable<Courier>> GetAllCouriersAsync()
    {
        return await _catalogsRepository.GetAllCouriers();
    }

    public async Task<Courier> AddCourierAsync(Courier courier)
    {
        return await _catalogsRepository.AddCourier(courier);
    }

    public async Task<Courier> UpdateCourierAsync(Courier courier)
    {
        return await _catalogsRepository.UpdateCourier(courier);
    }

    public async Task<bool> DeleteCourierAsync(Guid id)
    {
        return await _catalogsRepository.DeleteCourier(id);
    }

    // Coupon methods
    public async Task<Coupon?> GetCouponByIdAsync(Guid id)
    {
        return await _catalogsRepository.GetCouponById(id);
    }

    public async Task<Coupon?> GetCouponByCodeAsync(string code)
    {
        return await _catalogsRepository.GetCouponByCode(code);
    }

    public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
    {
        return await _catalogsRepository.GetAllCoupons();
    }

    public async Task<Coupon> AddCouponAsync(Coupon coupon)
    {
        return await _catalogsRepository.AddCoupon(coupon);
    }

    public async Task<Coupon> UpdateCouponAsync(Coupon coupon)
    {
        return await _catalogsRepository.UpdateCoupon(coupon);
    }

    public async Task<bool> DeleteCouponAsync(Guid id)
    {
        return await _catalogsRepository.DeleteCoupon(id);
    }

    // PricingOption methods
    public async Task<IEnumerable<ServicePricingOption>> GetPricingOptionsByServiceIdAsync(Guid serviceId)
        => await _catalogsRepository.GetPricingOptionsByServiceId(serviceId);

    public async Task<ServicePricingOption?> GetPricingOptionByIdAsync(Guid optionId)
        => await _catalogsRepository.GetPricingOptionById(optionId);

    public async Task<ServicePricingOption> AddPricingOptionAsync(ServicePricingOption option)
        => await _catalogsRepository.AddPricingOption(option);

    public async Task<ServicePricingOption> UpdatePricingOptionAsync(ServicePricingOption option)
        => await _catalogsRepository.UpdatePricingOption(option);

    public async Task<bool> DeletePricingOptionAsync(Guid optionId)
        => await _catalogsRepository.DeletePricingOption(optionId);

    public async Task<int> GetActivePricingOptionsCountByServiceIdAsync(Guid serviceId)
        => await _catalogsRepository.GetActivePricingOptionsCountByServiceId(serviceId);
}