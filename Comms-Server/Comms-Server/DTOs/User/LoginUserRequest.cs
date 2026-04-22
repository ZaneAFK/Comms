namespace Comms_Server.DTOs
{
	public class LoginUserRequest
	{
		public string Email { get; set; } = null!;
		public string Password { get; set; } = null!;
	}
}
