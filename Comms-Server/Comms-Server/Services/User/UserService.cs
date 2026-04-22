using Comms_Server.Database;
using Comms_Server.Database.Models;
using Comms_Server.DTOs;
using Comms_Server.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Comms_Server.Services
{
	public class UserService : Service, IUserService
	{
		private readonly UserManager<User> _userManager;

		public UserService(IFactory factory, UserManager<User> userManager) : base(factory)
		{
			_userManager = userManager;
		}

		public async Task<Result<User>> RegisterUserAsync(string username, string email, string password)
		{
			using var transaction = await Factory.BeginTransactionAsync();

			var user = new User
			{
				UserName = username,
				Email = email
			};

			var identityResult = await _userManager.CreateAsync(user, password);
			if (!identityResult.Succeeded)
			{
				var errors = identityResult.Errors.Select(e => e.Description);
				await transaction.RollbackAsync();
				return Result<User>.Failure(errors);
			}

			var roleResult = await _userManager.AddToRolesAsync(user, new[] { "User" });
			if (!roleResult.Succeeded)
			{
				var errors = roleResult.Errors.Select(e => e.Description);
				await transaction.RollbackAsync();
				return Result<User>.Failure(errors);
			}

			await transaction.CommitAsync();
			return Result<User>.Success(user);
		}

		public async Task<Result<User>> LoginAsync(string email, string password)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user is null)
			{
				return Result<User>.Failure(["Invalid email or password."]);
			}

			var passwordValid = await _userManager.CheckPasswordAsync(user, password);
			if (!passwordValid)
			{
				return Result<User>.Failure(["Invalid email or password."]);
			}

			return Result<User>.Success(user);
		}

		public async Task<User?> GetByIdAsync(Guid id)
		{
			return await _userManager.FindByIdAsync(id.ToString());
		}

		public async Task<IEnumerable<UserSearchDto>> SearchUsersAsync(string username)
		{
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
