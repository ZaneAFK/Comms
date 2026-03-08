using Comms_Server.Database;
using Comms_Server.Shared.Results;
using Microsoft.AspNetCore.Identity;
using UserModel = Comms_Server.Database.Models.User.User;

namespace Comms_Server.Services.User
{
	public class UserService : Service, IUserService
	{
		private readonly UserManager<UserModel> _userManager;

		public UserService(IFactory factory, UserManager<UserModel> userManager) : base(factory)
		{
			_userManager = userManager;
		}

		public async Task<Result<UserModel>> RegisterUserAsync(string username, string email, string password)
		{
			var user = new UserModel
			{
				UserName = username,
				Email = email
			};

			var identityResult = await _userManager.CreateAsync(user, password);

			if (!identityResult.Succeeded)
			{
				var errors = identityResult.Errors.Select(e => e.Description);
				return Result<UserModel>.Failure(errors);
			}

			await _userManager.AddToRolesAsync(user, new[] { "User" });

			return Result<UserModel>.Success(user);
		}

		public async Task<UserModel?> GetByIdAsync(Guid id)
		{
			return await Factory.GetAsync<UserModel>(id);
		}
	}
}
