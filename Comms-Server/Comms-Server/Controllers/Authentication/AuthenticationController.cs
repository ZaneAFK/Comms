using Comms_Server.DTOs;
using Comms_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comms_Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[AllowAnonymous]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAuthenticationService _authenticationService;

		public AuthenticationController(IAuthenticationService authenticationService)
		{
			_authenticationService = authenticationService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
		{
			var response = await _authenticationService.RegisterUserAsync(request.Username, request.Email, request.Password);

			if (!response.Succeeded)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
		{
			var response = await _authenticationService.LoginAsync(request.Email, request.Password);

			if (!response.Succeeded)
			{
				return Unauthorized(response);
			}

			return Ok(response);
		}
	}
}
