using Comms_Server.Controllers;
using Comms_Server.DTOs;
using Comms_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Controllers
{
	[TestFixture]
	public class UsersControllerTest
	{
		private Mock<IUserService> _userServiceMock = null!;
		private UsersController _controller = null!;

		[SetUp]
		public void Setup()
		{
			_userServiceMock = new Mock<IUserService>();
			_controller = new UsersController(_userServiceMock.Object);
		}

		[Test]
		public async Task Search_WithValidUsername_ReturnsOkWithUsers()
		{
			// Arrange
			var users = new List<UserSearchDto>
			{
				new() { Id = Guid.NewGuid(), Username = "TestUser" }
			};
			_userServiceMock
				.Setup(x => x.SearchUsersAsync("Test"))
				.ReturnsAsync(users);

			// Act
			var result = await _controller.Search("Test");

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult, "A valid username should result in an Ok response.");
			Assert.AreSame(users, okResult!.Value, "Ok response should contain the users returned by the service.");
		}

		[Test]
		public async Task Search_WithValidUsername_CallsServiceWithProvidedUsername()
		{
			// Arrange
			_userServiceMock
				.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
				.ReturnsAsync(new List<UserSearchDto>());

			// Act
			await _controller.Search("SomeUser");

			// Assert
			_userServiceMock.Verify(x => x.SearchUsersAsync("SomeUser"), Times.Once);
		}

		[Test]
		public async Task Search_WithNullUsername_ReturnsBadRequest()
		{
			// Act
			var result = await _controller.Search(null!);

			// Assert
			Assert.IsInstanceOf<BadRequestObjectResult>(result, "A null username should result in a BadRequest response.");
			_userServiceMock.Verify(x => x.SearchUsersAsync(It.IsAny<string>()), Times.Never);
		}

		[Test]
		public async Task Search_WithEmptyUsername_ReturnsBadRequest()
		{
			// Act
			var result = await _controller.Search(string.Empty);

			// Assert
			Assert.IsInstanceOf<BadRequestObjectResult>(result, "An empty username should result in a BadRequest response.");
			_userServiceMock.Verify(x => x.SearchUsersAsync(It.IsAny<string>()), Times.Never);
		}

		[Test]
		public async Task Search_WithWhitespaceUsername_ReturnsBadRequest()
		{
			// Act
			var result = await _controller.Search("   ");

			// Assert
			Assert.IsInstanceOf<BadRequestObjectResult>(result, "A whitespace-only username should result in a BadRequest response.");
			_userServiceMock.Verify(x => x.SearchUsersAsync(It.IsAny<string>()), Times.Never);
		}

		[Test]
		public async Task Search_WithNoMatches_ReturnsOkWithEmptyList()
		{
			// Arrange
			_userServiceMock
				.Setup(x => x.SearchUsersAsync("Nobody"))
				.ReturnsAsync(new List<UserSearchDto>());

			// Act
			var result = await _controller.Search("Nobody");

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var value = okResult!.Value as IEnumerable<UserSearchDto>;
			Assert.IsNotNull(value);
			Assert.IsEmpty(value!, "Ok response should contain an empty list when no users match.");
		}
	}
}
