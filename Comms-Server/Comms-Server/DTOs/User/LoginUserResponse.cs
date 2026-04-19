namespace Comms_Server.DTOs
{
	public class LoginUserResponse
	{
		public bool Succeeded { get; set; }
		public string? Token { get; set; }
		public UserDto? User { get; set; }
		public string? Error { get; set; }
	}
}
