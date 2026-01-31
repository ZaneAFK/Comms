using Comms_Server.Database.Models.User;
using Comms_Server.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Comms_Server.Testing.Services.Authentication
{
	[TestFixture]
	public class AuthManagerTest
	{
		[Test]
		public async Task RegisterSecurityUserAsync_ReturnsSuccess_AndAddsUserRole_WhenCreateSucceeds()
		{
			// Arrange
			var mockUserManager = SetupUserManagerMock();

			mockUserManager
				.Setup(um => um.CreateAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);

			mockUserManager
				.Setup(um => um.AddToRolesAsync(It.IsAny<SecurityUser>(), It.IsAny<IEnumerable<string>>()))
				.ReturnsAsync(IdentityResult.Success);

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };

			// Act
			var result = await authManager.RegisterSecurityUserAsync(user, "Password123!");

			// Assert
			Assert.That(result.Succeeded, Is.True);

			mockUserManager.Verify(
				um => um.CreateAsync(user, "Password123!"),
				Times.Once);

			mockUserManager.Verify(
				um => um.AddToRolesAsync(
					user,
					It.Is<IEnumerable<string>>(r => r.SequenceEqual(new[] { "User" }))
				),
				Times.Once);
		}

		[Test]
		public async Task RegisterSecurityUserAsync_ReturnsFailure_AndDoesNotAddRole_WhenCreateFails()
		{
			// Arrange
			var mockUserManager = SetupUserManagerMock();

			var error = new IdentityError { Description = "Invalid password" };

			mockUserManager
				.Setup(um => um.CreateAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(error));

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };

			// Act
			var result = await authManager.RegisterSecurityUserAsync(user, "bad");

			// Assert
			Assert.That(result.Succeeded, Is.False);
			Assert.That(result.Errors.Any(e => e.Description == "Invalid password"));

			mockUserManager.Verify(
				um => um.CreateAsync(user, "bad"),
				Times.Once);

			// CRITICAL: roles must NOT be added on failure
			mockUserManager.Verify(
				um => um.AddToRolesAsync(It.IsAny<SecurityUser>(), It.IsAny<IEnumerable<string>>()),
				Times.Never);
		}

		[Test]
		public void RegisterSecurityUserAsync_PropagatesException_WhenUserManagerThrows()
		{
			// Arrange
			var mockUserManager = SetupUserManagerMock();

			mockUserManager
				.Setup(um => um.CreateAsync(It.IsAny<SecurityUser>(), It.IsAny<string>()))
				.ThrowsAsync(new InvalidOperationException("boom"));

			var authManager = new AuthManager(mockUserManager.Object);
			var user = new SecurityUser { UserName = "alice", Email = "alice@example.com" };

			// Act + Assert
			var ex = Assert.ThrowsAsync<InvalidOperationException>(
				async () => await authManager.RegisterSecurityUserAsync(user, "Password123!")
			);

			Assert.That(ex!.Message, Is.EqualTo("boom"));

			// Ensure roles were never added
			mockUserManager.Verify(
				um => um.AddToRolesAsync(It.IsAny<SecurityUser>(), It.IsAny<IEnumerable<string>>()),
				Times.Never);
		}

		private static Mock<UserManager<SecurityUser>> SetupUserManagerMock()
		{
			var store = new Mock<IUserStore<SecurityUser>>();

#pragma warning disable CS8625
			return new Mock<UserManager<SecurityUser>>(
				store.Object,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			);
#pragma warning restore CS8625
		}
	}
}
