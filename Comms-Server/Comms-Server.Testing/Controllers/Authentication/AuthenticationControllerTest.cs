using Comms_Server.Controllers;
using Comms_Server.DTOs;
using Comms_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Controllers
{
	[TestFixture]
	public class AuthenticationControllerTest
	{
		private Mock<IAuthenticationService> _authenticationServiceMock = null!;
		private AuthenticationController _controller = null!;

		[SetUp]
		public void Setup()
		{
			_authenticationServiceMock = new Mock<IAuthenticationService>();
			_controller = new AuthenticationController(_authenticationServiceMock.Object);
		}

		[Test]
		public async Task Register_WithValidRequest_ReturnsServiceResponse()
		{
			// Arrange
			var expectedResponse = new RegisterUserResponse { Succeeded = true };
			_authenticationServiceMock
				.Setup(x => x.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!"))
				.ReturnsAsync(expectedResponse);

			var request = new RegisterUserRequest
			{
				Username = "TestUser",
				Email = "testuser@hotmail.com",
				Password = "supersecure123!"
			};

			// Act
			var result = await _controller.Register(request);

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult, "A successful registration should result in an Ok response.");
			Assert.AreSame(expectedResponse, okResult!.Value, "Controller should return the response produced by the authentication service.");
		}

		[Test]
		public async Task Register_WithValidRequest_CallsServiceWithRequestValues()
		{
			// Arrange
			_authenticationServiceMock
				.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new RegisterUserResponse { Succeeded = true });

			var request = new RegisterUserRequest
			{
				Username = "TestUser",
				Email = "testuser@hotmail.com",
				Password = "supersecure123!"
			};

			// Act
			await _controller.Register(request);

			// Assert
			_authenticationServiceMock.Verify(
				x => x.RegisterUserAsync("TestUser", "testuser@hotmail.com", "supersecure123!"),
				Times.Once);
		}

		[Test]
		public async Task Register_WhenServiceFails_ReturnsBadRequestWithFailureResponse()
		{
			// Arrange
			var expectedResponse = new RegisterUserResponse { Succeeded = false, Error = "Email already in use." };
			_authenticationServiceMock
				.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResponse);

			var request = new RegisterUserRequest
			{
				Username = "TestUser",
				Email = "testuser@hotmail.com",
				Password = "supersecure123!"
			};

			// Act
			var result = await _controller.Register(request);

			// Assert
			var badRequestResult = result as BadRequestObjectResult;
			Assert.IsNotNull(badRequestResult, "A failed registration should result in a BadRequest response.");
			var response = badRequestResult!.Value as RegisterUserResponse;
			Assert.IsNotNull(response);
			Assert.IsFalse(response!.Succeeded, "Controller should propagate a failed registration result.");
			Assert.AreEqual("Email already in use.", response.Error);
		}

		[Test]
		public async Task Login_WithValidRequest_ReturnsServiceResponse()
		{
			// Arrange
			var expectedResponse = new LoginUserResponse { Succeeded = true, Token = "jwt-token" };
			_authenticationServiceMock
				.Setup(x => x.LoginAsync("testuser@hotmail.com", "supersecure123!"))
				.ReturnsAsync(expectedResponse);

			var request = new LoginUserRequest
			{
				Email = "testuser@hotmail.com",
				Password = "supersecure123!"
			};

			// Act
			var result = await _controller.Login(request);

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult, "A successful login should result in an Ok response.");
			Assert.AreSame(expectedResponse, okResult!.Value, "Controller should return the response produced by the authentication service.");
		}

		[Test]
		public async Task Login_WithValidRequest_CallsServiceWithRequestValues()
		{
			// Arrange
			_authenticationServiceMock
				.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new LoginUserResponse { Succeeded = true });

			var request = new LoginUserRequest
			{
				Email = "testuser@hotmail.com",
				Password = "supersecure123!"
			};

			// Act
			await _controller.Login(request);

			// Assert
			_authenticationServiceMock.Verify(
				x => x.LoginAsync("testuser@hotmail.com", "supersecure123!"),
				Times.Once);
		}

		[Test]
		public async Task Login_WhenServiceFails_ReturnsUnauthorizedWithFailureResponse()
		{
			// Arrange
			var expectedResponse = new LoginUserResponse { Succeeded = false, Error = "Invalid credentials." };
			_authenticationServiceMock
				.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResponse);

			var request = new LoginUserRequest
			{
				Email = "testuser@hotmail.com",
				Password = "wrongpassword"
			};

			// Act
			var result = await _controller.Login(request);

			// Assert
			var unauthorizedResult = result as UnauthorizedObjectResult;
			Assert.IsNotNull(unauthorizedResult, "A failed login should result in an Unauthorized response.");
			var response = unauthorizedResult!.Value as LoginUserResponse;
			Assert.IsNotNull(response);
			Assert.IsFalse(response!.Succeeded, "Controller should propagate a failed login result.");
			Assert.IsNull(response.Token);
			Assert.AreEqual("Invalid credentials.", response.Error);
		}
	}
}
