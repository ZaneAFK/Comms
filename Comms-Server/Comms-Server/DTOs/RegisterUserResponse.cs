namespace Comms_Server.DTOs
{
	public class RegisterUserResponse
	{
		public Guid UserId { get; set; }
		public string Username { get; set; } = null!;
		public string Email { get; set; } = null!;
	}
}
