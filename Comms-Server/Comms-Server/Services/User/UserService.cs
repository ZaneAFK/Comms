using Comms_Server.Database;
using Comms_Server.Database.Models.User;
using Comms_Server.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace Comms_Server.Services.User
{
	public class UserService : Service, IUserService
	{
		private readonly UserManager<AppUser> _userManager;

		public UserService(IFactory factory, UserManager<AppUser> userManager) : base(factory)
		{
			_userManager = userManager;
		}

		public async Task<Result<AppUser>> RegisterUserAsync(string username, string email, string password)
		{
			var user = new AppUser
			{
				UserName = username,
				Email = email
			};

			var identityResult = await _userManager.CreateAsync(user, password);
			if (!identityResult.Succeeded)
			{
				var errors = identityResult.Errors.Select(e => e.Description);
				return Result<AppUser>.Failure(errors);
			}

			var roleResult = await _userManager.AddToRolesAsync(user, new[] { "User" });
			if (!roleResult.Succeeded)
			{
				var errors = roleResult.Errors.Select(e => e.Description);
				return Result<AppUser>.Failure(errors);
			}

			return Result<AppUser>.Success(user);
		}

		public async Task<AppUser?> GetByIdAsync(Guid id)
		{
			return await _userManager.FindByIdAsync(id.ToString());
		}
	}
}
