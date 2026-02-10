using Microsoft.AspNetCore.Mvc;
using Catalogs.Infrastructure.Services;
using Catalogs.Domain.Entities;

namespace Catalogs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly CatalogsService _catalogsService;

    public CatalogsController(CatalogsService catalogsService)
    {
        _catalogsService = catalogsService;
    }

    // Services
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var services = await _catalogsService.GetAllServicesAsync();
        return Ok(new { services });
    }

    [HttpGet("services/{id}")]
    public async Task<IActionResult> GetService(Guid id)
    {
        var service = await _catalogsService.GetServiceByIdAsync(id);
        return Ok(new { service });
    }

    [HttpPost("services")]
    public async Task<IActionResult> AddService([FromBody] Service service)
    {
        var createdService = await _catalogsService.AddServiceAsync(service);
        return Created(string.Empty, new { message = "Service added successfully", data = createdService });
    }

    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] Service service)
    {
        service.Id = id;
        var updatedService = await _catalogsService.UpdateServiceAsync(service);
        return Ok(new { message = "Service updated successfully", data = updatedService });
    }

    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var result = await _catalogsService.DeleteServiceAsync(id);
        return Ok(new { message = "Service deleted successfully", success = result });
    }

    // Couriers
    [HttpGet("couriers")]
    public async Task<IActionResult> GetCouriers()
    {
        var couriers = await _catalogsService.GetAllCouriersAsync();
        return Ok(new { couriers });
    }

    [HttpGet("couriers/{id}")]
    public async Task<IActionResult> GetCourier(Guid id)
    {
        var courier = await _catalogsService.GetCourierByIdAsync(id);
        return Ok(new { courier });
    }

    [HttpPost("couriers")]
    public async Task<IActionResult> AddCourier([FromBody] Courier courier)
    {
        var createdCourier = await _catalogsService.AddCourierAsync(courier);
        return Created(string.Empty, new { message = "Courier added successfully", data = createdCourier });
    }

    [HttpPut("couriers/{id}")]
    public async Task<IActionResult> UpdateCourier(Guid id, [FromBody] Courier courier)
    {
        courier.Id = id;
        var updatedCourier = await _catalogsService.UpdateCourierAsync(courier);
        return Ok(new { message = "Courier updated successfully", data = updatedCourier });
    }

    [HttpDelete("couriers/{id}")]
    public async Task<IActionResult> DeleteCourier(Guid id)
    {
        var result = await _catalogsService.DeleteCourierAsync(id);
        return Ok(new { message = "Courier deleted successfully", success = result });
    }
}