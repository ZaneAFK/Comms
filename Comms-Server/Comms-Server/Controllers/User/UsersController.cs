using Comms_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comms_Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] string username)
		{
			if (string.IsNullOrWhiteSpace(username))
			{
				return BadRequest("Username is required.");
			}

			var users = await _userService.SearchUsersAsync(username);

			return Ok(users);
		}
	}
}
