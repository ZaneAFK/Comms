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
		public async Task<RegisterUserResponse> Register([FromBody] RegisterUserRequest request)
		{
			return await _authenticationService.RegisterUserAsync(request.Username, request.Email, request.Password);
		}

		[HttpPost("login")]
		public async Task<LoginUserResponse> Login([FromBody] LoginUserRequest request)
		{
			return await _authenticationService.LoginAsync(request.Email, request.Password);
		}
	}
}
