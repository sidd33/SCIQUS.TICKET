using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/accounts/{accountId}/addresses")]
    public class AccountAddressesController : ControllerBase
    {
        private readonly IAccountAddressService _addressService;

        public AccountAddressesController(IAccountAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByAccountId(string accountId)
        {
            var addresses = await _addressService.GetAddressesByAccountIdAsync(accountId);
            return Ok(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(string accountId, [FromBody] AccountAddressResponse request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _addressService.AddAddressAsync(accountId, request);
            return Ok(created);
        }

        [HttpPut("{addressId}/set-primary")]
        public async Task<IActionResult> SetPrimaryAddress(string accountId, Guid addressId)
        {
            var success = await _addressService.SetPrimaryAddressAsync(accountId, addressId);
            if (!success) return NotFound(new { message = "Address or Account not found." });

            return Ok(new { message = "Primary address updated successfully." });
        }

        [HttpDelete("{addressId}")]
        public async Task<IActionResult> SoftDeleteAddress(Guid addressId)
        {
            var deleted = await _addressService.SoftDeleteAddressAsync(addressId);
            if (!deleted) return NotFound(new { message = "Address not found." });

            return Ok(new { message = "Address soft-deleted successfully." });
        }
    }
}
