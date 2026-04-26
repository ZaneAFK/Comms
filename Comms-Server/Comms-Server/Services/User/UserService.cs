using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.DTOs;
using Comms_Server.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Services
{
	public class UserService : Service<UserService>, IUserService
	{
		private readonly UserManager<User> _userManager;

		public UserService(IFactory factory, ILogger<UserService> logger, UserManager<User> userManager) : base(factory, logger)
		{
			_userManager = userManager;
		}

		public async Task<Result<User>> RegisterUserAsync(string username, string email, string password)
		{
			Logger.LogDebug("Creating account for username '{Username}', email '{Email}'", username, email);

			using var transaction = await Factory.BeginTransactionAsync();

			var user = new User
			{
				UserName = username,
				Email = email
			};

			var identityResult = await _userManager.CreateAsync(user, password);
			if (!identityResult.Succeeded)
			{
				var errors = identityResult.Errors.Select(e => e.Description).ToList();
				Logger.LogWarning("Identity failed to create user '{Username}': {Errors}", username, string.Join(", ", errors));
				await transaction.RollbackAsync();
				return Result<User>.Failure(errors);
			}

			var roleResult = await _userManager.AddToRolesAsync(user, new[] { "User" });
			if (!roleResult.Succeeded)
			{
				var errors = roleResult.Errors.Select(e => e.Description).ToList();
				Logger.LogWarning("Role assignment failed for user '{Username}': {Errors}", username, string.Join(", ", errors));
				await transaction.RollbackAsync();
				return Result<User>.Failure(errors);
			}

			await transaction.CommitAsync();
			Logger.LogInformation("User '{Username}' created with ID {UserId} and assigned 'User' role", username, user.Id);
			return Result<User>.Success(user);
		}

		public async Task<Result<User>> LoginAsync(string email, string password)
		{
			Logger.LogDebug("Validating credentials for email '{Email}'", email);

			var user = await _userManager.FindByEmailAsync(email);
			if (user is null)
			{
				Logger.LogDebug("Login failed for email '{Email}'", email);
				return Result<User>.Failure(["Invalid email or password."]);
			}

			var passwordValid = await _userManager.CheckPasswordAsync(user, password);
			if (!passwordValid)
			{
				Logger.LogDebug("Login failed for email '{Email}'", email);
				return Result<User>.Failure(["Invalid email or password."]);
			}

			return Result<User>.Success(user);
		}

		public async Task<User?> GetByIdAsync(Guid id)
		{
			Logger.LogDebug("Looking up user by ID {UserId}", id);
			return await _userManager.FindByIdAsync(id.ToString());
		}

		public async Task<IEnumerable<UserSearchDto>> SearchUsersAsync(string username)
		{
			Logger.LogDebug("Searching users with query '{Query}'", username);
			return await _userManager.Users
				.Where(u => u.UserName != null && u.UserName.Contains(username))
				.Take(10)
				.Select(u => new UserSearchDto
				{
					Id = u.Id,
					Username = u.UserName!
				})
				.ToListAsync();
		}
	}
}
