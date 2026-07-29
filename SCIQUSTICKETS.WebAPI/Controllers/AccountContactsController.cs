using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/accounts/{accountId}/contacts")]
    public class AccountContactsController : ControllerBase
    {
        private readonly IAccountContactService _contactService;

        public AccountContactsController(IAccountContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByAccountId(string accountId)
        {
            var contacts = await _contactService.GetContactsByAccountIdAsync(accountId);
            return Ok(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(string accountId, [FromBody] AccountContactResponse request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _contactService.AddContactAsync(accountId, request);
            return Ok(created);
        }

        [HttpPut("{contactId}/set-primary")]
        public async Task<IActionResult> SetPrimaryContact(string accountId, Guid contactId)
        {
            var success = await _contactService.SetPrimaryContactAsync(accountId, contactId);
            if (!success) return NotFound(new { message = "Contact or Account not found." });

            return Ok(new { message = "Primary contact updated successfully." });
        }

        [HttpDelete("{contactId}")]
        public async Task<IActionResult> SoftDeleteContact(Guid contactId)
        {
            var deleted = await _contactService.SoftDeleteContactAsync(contactId);
            if (!deleted) return NotFound(new { message = "Contact not found." });

            return Ok(new { message = "Contact soft-deleted successfully." });
        }
    }
}
