using Catalogs.Domain.Entities;

namespace Catalogs.Infrastructure.Services
{
    public interface ICatalogsRepository
    {
        Task<Service> GetServiceById(int id);
        Task<IEnumerable<Service>> GetAllServices();
        Task<Service> AddService(Service service);
        Task<Service> UpdateService(Service service);
        Task<bool> DeleteService(int id);


        Task<Courier> GetCourierById(int id);
        Task<IEnumerable<Courier>> GetAllCouriers();
        Task<Courier> AddCourier(Courier courier);
        Task<Courier> UpdateCourier(Courier courier);
        Task<bool> DeleteCourier(int id);
    }
}