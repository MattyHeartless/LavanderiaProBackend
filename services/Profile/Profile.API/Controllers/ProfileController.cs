using Microsoft.AspNetCore.Mvc;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;

namespace Profile.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileRepository _profileRepository;

    public ProfileController(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    // Addresses
    [HttpGet("addresses/{userId}")]
    public async Task<IActionResult> GetAddresses(Guid userId)
    {
        var addresses = await _profileRepository.GetAddressesAsync(userId);
        return Ok(new { addresses });
    }

    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress([FromBody] UserAddress address)
    {
        await _profileRepository.AddAddressAsync(address);
        return Created(string.Empty, new { message = "Address added successfully" });
    }

    [HttpPut("addresses/{addressId}")]
    public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UserAddress address)
    {
        address.Id = addressId;
        await _profileRepository.UpdateAddressAsync(address);
        return Ok(new { message = "Address updated successfully" });
    }

    [HttpDelete("addresses/{addressId}/{userId}")]
    public async Task<IActionResult> DeleteAddress(int addressId, Guid userId)
    {
        await _profileRepository.DeleteAddressAsync(addressId, userId);
        return Ok(new { message = "Address deleted successfully" });
    }

    // Payment Methods
    [HttpGet("payment-methods/{userId}")]
    public async Task<IActionResult> GetPaymentMethods(Guid userId)
    {
        var methods = await _profileRepository.GetPaymentMethodsAsync(userId);
        return Ok(new { message = "Payment methods retrieved successfully", data = methods });
    }

    [HttpPost("payment-methods")]
    public async Task<IActionResult> AddPaymentMethod([FromBody] UserPaymentMethod method)
    {
        await _profileRepository.AddPaymentMethodAsync(method);
        return Created(string.Empty, new { message = "Payment method added successfully" });
    }

    [HttpDelete("payment-methods/{methodId}/{userId}")]
    public async Task<IActionResult> DeletePaymentMethod(int methodId, Guid userId)
    {
        await _profileRepository.DeletePaymentMethodAsync(methodId, userId);
        return Ok(new { message = "Payment method deleted successfully" });
    }

    [HttpPut("payment-methods/{methodId}")]
    public async Task<IActionResult> UpdatePaymentMethod(int methodId, [FromBody] UserPaymentMethod method)
    {
        method.Id = methodId;
        await _profileRepository.UpdatePaymentMethodAsync(method);
        return Ok(new { message = "Payment method updated successfully" });
    }
}