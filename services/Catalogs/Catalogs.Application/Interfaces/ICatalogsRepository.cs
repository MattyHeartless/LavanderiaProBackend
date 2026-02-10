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
        Task<IEnumerable<Courier>> GetAllCouriers();
        Task<Courier> AddCourier(Courier courier);
        Task<Courier> UpdateCourier(Courier courier);
        Task<bool> DeleteCourier(Guid id);
    }
}