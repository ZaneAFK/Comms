using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Comms_Server.Database.Models;
using Microsoft.IdentityModel.Tokens;

namespace Comms_Server.Services
{
	public class JwtService : IJwtService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<JwtService> _logger;

		public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public string GenerateToken(User user)
		{
			_logger.LogDebug("Generating JWT token for user {UserId}", user.Id);

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email!),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!)
			};

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
