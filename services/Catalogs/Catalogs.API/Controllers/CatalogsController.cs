using Microsoft.AspNetCore.Mvc;
using Catalogs.API.DTOs;
using Catalogs.Infrastructure.Services;
using Catalogs.Domain.Entities;

namespace Catalogs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly CatalogsService _catalogsService;
    private readonly IFileStorageService _fileStorageService;

    public CatalogsController(CatalogsService catalogsService, IFileStorageService fileStorageService)
    {
        _catalogsService = catalogsService;
        _fileStorageService = fileStorageService;
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

    // Pricing Options
    [HttpGet("services/{serviceId}/pricing-options")]
    public async Task<IActionResult> GetPricingOptions(Guid serviceId)
    {
        var options = await _catalogsService.GetPricingOptionsByServiceIdAsync(serviceId);
        return Ok(new { pricingOptions = options });
    }

    [HttpGet("pricing-options/{optionId}/is-active")]
    public async Task<IActionResult> GetPricingOptionIsActive(Guid optionId)
    {
        var option = await _catalogsService.GetPricingOptionByIdAsync(optionId);
        if (option == null)
            return NotFound(new { message = "Pricing option not found" });

        return Ok(new { isActive = option.IsActive });
    }

    [HttpPost("services/{serviceId}/pricing-options")]
    public async Task<IActionResult> AddPricingOption(Guid serviceId, [FromBody] CreatePricingOptionRequest request)
    {
        var service = await _catalogsService.GetServiceByIdAsync(serviceId);
        if (service == null)
            return NotFound(new { message = "Service not found" });

        var validationError = ServicePricingOptionRules.Validate(request.OptionName, request.Price, request.UoM);
        if (validationError != null)
            return BadRequest(new { message = validationError });

        var existing = await _catalogsService.GetPricingOptionsByServiceIdAsync(serviceId);
        if (existing.Any(o => o.OptionName == request.OptionName))
            return BadRequest(new { message = "A pricing option with that name already exists for this service" });

        var now = DateTime.UtcNow;
        var option = new ServicePricingOption
        {
            ServiceId  = serviceId,
            OptionName = request.OptionName,
            Price      = request.Price,
            UoM        = request.UoM,
            IsActive   = request.IsActive,
            CreatedAt  = now,
            UpdatedAt  = now,
        };

        var created = await _catalogsService.AddPricingOptionAsync(option);
        return Created(string.Empty, new { message = "Pricing option added successfully", data = created });
    }

    [HttpPut("services/{serviceId}/pricing-options/{optionId}")]
    public async Task<IActionResult> UpdatePricingOption(Guid serviceId, Guid optionId, [FromBody] UpdatePricingOptionRequest request)
    {
        var validationError = ServicePricingOptionRules.Validate(request.OptionName, request.Price, request.UoM);
        if (validationError != null)
            return BadRequest(new { message = validationError });

        var existing = await _catalogsService.GetPricingOptionByIdAsync(optionId);
        if (existing == null || existing.ServiceId != serviceId)
            return NotFound(new { message = "Pricing option not found" });

        if (!request.IsActive && existing.IsActive)
        {
            var activeCount = await _catalogsService.GetActivePricingOptionsCountByServiceIdAsync(serviceId);
            if (activeCount <= 1)
                return BadRequest(new { message = "Service must have at least one active pricing option" });
        }

        if (existing.OptionName != request.OptionName)
        {
            var siblings = await _catalogsService.GetPricingOptionsByServiceIdAsync(serviceId);
            if (siblings.Any(o => o.Id != optionId && o.OptionName == request.OptionName))
                return BadRequest(new { message = "A pricing option with that name already exists for this service" });
        }

        existing.OptionName = request.OptionName;
        existing.Price      = request.Price;
        existing.UoM        = request.UoM;
        existing.IsActive   = request.IsActive;
        existing.UpdatedAt  = DateTime.UtcNow;

        var updated = await _catalogsService.UpdatePricingOptionAsync(existing);
        return Ok(new { message = "Pricing option updated successfully", data = updated });
    }

    [HttpDelete("services/{serviceId}/pricing-options/{optionId}")]
    public async Task<IActionResult> DeletePricingOption(Guid serviceId, Guid optionId)
    {
        var existing = await _catalogsService.GetPricingOptionByIdAsync(optionId);
        if (existing == null || existing.ServiceId != serviceId)
            return NotFound(new { message = "Pricing option not found" });

        if (existing.IsActive)
        {
            var activeCount = await _catalogsService.GetActivePricingOptionsCountByServiceIdAsync(serviceId);
            if (activeCount <= 1)
                return BadRequest(new { message = "Cannot delete the only active pricing option of a service" });
        }

        var result = await _catalogsService.DeletePricingOptionAsync(optionId);
        return Ok(new { message = "Pricing option deleted successfully", success = result });
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
        if (courier == null)
            return NotFound(new { message = "Courier not found" });

        return Ok(new { courier });
    }

    [HttpGet("couriers/by-auth-user/{authUserId}")]
    public async Task<IActionResult> GetCourierByAuthUserId(string authUserId)
    {
        var courier = await _catalogsService.GetCourierByAuthUserIdAsync(authUserId);
        if (courier == null)
            return NotFound(new { message = "Courier not found" });

        return Ok(new { courier });
    }

    [HttpPost("couriers/{courierId}/profile-image")]
    [ProducesResponseType(typeof(UploadProfileImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadCourierProfileImage(Guid courierId, [FromForm] UploadProfileImageRequest request)
    {
        if (request.File == null)
            return BadRequest(new { message = "File is required" });

        var courier = await _catalogsService.GetCourierByIdAsync(courierId);
        if (courier == null)
            return NotFound(new { message = "Courier not found" });

        try
        {
            var previousImage = courier.ProfileImageUrl;
            var storedFile = await _fileStorageService.SaveCourierProfileImageAsync(request.File);

            courier.ProfileImageUrl = storedFile.PublicUrl;
            await _catalogsService.UpdateCourierAsync(courier);

            if (!string.IsNullOrWhiteSpace(previousImage))
            {
                await _fileStorageService.DeleteByRelativePathAsync(previousImage);
            }

            return Ok(new UploadProfileImageResponse
            {
                Message = "Profile image uploaded",
                ImageUrl = storedFile.PublicUrl,
                RelativePath = storedFile.RelativePath
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error uploading profile image" });
        }
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

    // Coupons
    [HttpGet("coupons")]
    public async Task<IActionResult> GetCoupons()
    {
        var coupons = await _catalogsService.GetAllCouponsAsync();
        return Ok(new { coupons });
    }

    [HttpGet("coupons/{id}")]
    public async Task<IActionResult> GetCoupon(Guid id)
    {
        var coupon = await _catalogsService.GetCouponByIdAsync(id);
        if (coupon == null)
            return NotFound(new { message = "Coupon not found" });

        return Ok(new { coupon });
    }

    [HttpPost("coupons")]
    public async Task<IActionResult> AddCoupon([FromBody] CreateCouponRequest request)
    {
        var validationError = CouponRules.ValidateRequest(
            request.Code,
            request.Name,
            request.BenefitType,
            request.BenefitValue,
            request.EventType,
            request.ExpiresAt,
            request.UsageLimit,
            DateTime.UtcNow);

        if (validationError != null)
            return BadRequest(new { message = validationError });

        var existingCoupon = await _catalogsService.GetCouponByCodeAsync(request.Code.Trim());
        if (existingCoupon != null)
            return BadRequest(new { message = "A coupon with that code already exists" });

        var now = DateTime.UtcNow;
        var coupon = new Coupon
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            BenefitType = request.BenefitType.Trim(),
            BenefitValue = request.BenefitValue,
            EventType = request.EventType.Trim(),
            IsActive = request.IsActive,
            ExpiresAt = request.ExpiresAt,
            UsageLimit = request.UsageLimit,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdCoupon = await _catalogsService.AddCouponAsync(coupon);
        return Created(string.Empty, new { message = "Coupon added successfully", data = createdCoupon });
    }

    [HttpPut("coupons/{id}")]
    public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] UpdateCouponRequest request)
    {
        var validationError = CouponRules.ValidateRequest(
            request.Code,
            request.Name,
            request.BenefitType,
            request.BenefitValue,
            request.EventType,
            request.ExpiresAt,
            request.UsageLimit,
            DateTime.UtcNow);

        if (validationError != null)
            return BadRequest(new { message = validationError });

        var existingCoupon = await _catalogsService.GetCouponByIdAsync(id);
        if (existingCoupon == null)
            return NotFound(new { message = "Coupon not found" });

        var repeatedCode = await _catalogsService.GetCouponByCodeAsync(request.Code.Trim());
        if (repeatedCode != null && repeatedCode.Id != id)
            return BadRequest(new { message = "A coupon with that code already exists" });

        existingCoupon.Code = request.Code.Trim();
        existingCoupon.Name = request.Name.Trim();
        existingCoupon.Description = request.Description?.Trim();
        existingCoupon.BenefitType = request.BenefitType.Trim();
        existingCoupon.BenefitValue = request.BenefitValue;
        existingCoupon.EventType = request.EventType.Trim();
        existingCoupon.IsActive = request.IsActive;
        existingCoupon.ExpiresAt = request.ExpiresAt;
        existingCoupon.UsageLimit = request.UsageLimit;
        existingCoupon.UpdatedAt = DateTime.UtcNow;

        var updatedCoupon = await _catalogsService.UpdateCouponAsync(existingCoupon);
        return Ok(new { message = "Coupon updated successfully", data = updatedCoupon });
    }

    [HttpDelete("coupons/{id}")]
    public async Task<IActionResult> DeleteCoupon(Guid id)
    {
        var result = await _catalogsService.DeleteCouponAsync(id);
        if (!result)
            return NotFound(new { message = "Coupon not found" });

        return Ok(new { message = "Coupon deleted successfully", success = true });
    }

}