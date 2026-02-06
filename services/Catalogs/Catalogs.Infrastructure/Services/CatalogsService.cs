
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
    public async Task<Service> GetServiceByIdAsync(int id)
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

    public async Task<bool> DeleteServiceAsync(int id)
    {
        return await _catalogsRepository.DeleteService(id);
    }

    // Courier methods
    public async Task<Courier> GetCourierByIdAsync(int id)
    {
        return await _catalogsRepository.GetCourierById(id);
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

    public async Task<bool> DeleteCourierAsync(int id)
    {
        return await _catalogsRepository.DeleteCourier(id);
    }
}