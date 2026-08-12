using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AccountRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/Accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AccountQueryParams queryParams)
        {
            var result = await _accountService.GetAllAccountsAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null) return NotFound(new { message = $"Account with ID {id} not found." });

            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.AccountId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateAccountRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = await _accountService.UpdateAccountAsync(id, request);
                if (updated == null) return NotFound(new { message = $"Account with ID {id} not found." });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var deleted = await _accountService.SoftDeleteAccountAsync(id);
            if (!deleted) return NotFound(new { message = $"Account with ID {id} not found." });

            return Ok(new { message = $"Account {id} soft-deleted successfully." });
        }
    }
}
