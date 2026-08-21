using Microsoft.AspNetCore.Identity;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class SystemActorService : ISystemActorService
	{
		private const string SystemUserEmail = "admin@sciqustickets.com";

		private readonly UserManager<ApplicationUser> _userManager;

		public SystemActorService(
			UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<string> GetSystemActorIdAsync()
		{
			var systemUser = await _userManager
				.FindByEmailAsync(SystemUserEmail);

			if (systemUser == null)
			{
				throw new InvalidOperationException(
					$"System user '{SystemUserEmail}' was not found.");
			}

			return systemUser.Id;
		}
	}
}