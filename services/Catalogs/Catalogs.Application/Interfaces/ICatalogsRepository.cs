using Catalogs.Domain.Entities;

namespace Catalogs.Infrastructure.Services
{
    public interface ICatalogsRepository
    {
        Task<Service> GetServiceById(Guid id);
        Task<IEnumerable<Service>> GetAllServices();
        Task<Service> AddService(Service service);
        Task<Service> UpdateService(Service service);
        Task<bool> DeleteService(Guid id);


        Task<Courier> GetCourierById(Guid id);
        Task<Courier> GetCourierByAuthUserId(string authUserId);
        Task<IEnumerable<Courier>> GetAllCouriers();
        Task<Courier> AddCourier(Courier courier);
        Task<Courier> UpdateCourier(Courier courier);
        Task<bool> DeleteCourier(Guid id);

        Task<Coupon?> GetCouponById(Guid id);
        Task<Coupon?> GetCouponByCode(string code);
        Task<IEnumerable<Coupon>> GetAllCoupons();
        Task<Coupon> AddCoupon(Coupon coupon);
        Task<Coupon> UpdateCoupon(Coupon coupon);
        Task<bool> DeleteCoupon(Guid id);
    }
}