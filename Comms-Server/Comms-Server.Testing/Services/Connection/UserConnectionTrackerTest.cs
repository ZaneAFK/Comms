using Comms_Server.Services;
using NUnit.Framework;

namespace Comms_Server.Testing.Services
{
	[TestFixture]
	public class UserConnectionTrackerTest
	{
		private UserConnectionTracker _tracker = null!;

		[SetUp]
		public void Setup()
		{
			_tracker = new UserConnectionTracker();
		}

		[Test]
		public void GetConnections_ForUnknownUser_ReturnsEmpty()
		{
			// Act
			var connections = _tracker.GetConnections(Guid.NewGuid());

			// Assert
			Assert.IsEmpty(connections, "A user with no tracked connections should return an empty collection.");
		}

		[Test]
		public void AddConnection_SingleConnection_IsReturnedByGetConnections()
		{
			// Arrange
			var userId = Guid.NewGuid();

			// Act
			_tracker.AddConnection(userId, "conn-1");

			// Assert
			CollectionAssert.AreEquivalent(new[] { "conn-1" }, _tracker.GetConnections(userId));
		}

		[Test]
		public void AddConnection_MultipleConnectionsForSameUser_AllAreReturned()
		{
			// Arrange
			var userId = Guid.NewGuid();

			// Act
			_tracker.AddConnection(userId, "conn-1");
			_tracker.AddConnection(userId, "conn-2");

			// Assert
			CollectionAssert.AreEquivalent(new[] { "conn-1", "conn-2" }, _tracker.GetConnections(userId));
		}

		[Test]
		public void AddConnection_SameConnectionIdTwice_IsNotDuplicated()
		{
			// Arrange
			var userId = Guid.NewGuid();

			// Act
			_tracker.AddConnection(userId, "conn-1");
			_tracker.AddConnection(userId, "conn-1");

			// Assert
			Assert.AreEqual(1, _tracker.GetConnections(userId).Count, "Adding the same connection twice should not duplicate it.");
		}

		[Test]
		public void AddConnection_DifferentUsers_AreTrackedSeparately()
		{
			// Arrange
			var userId1 = Guid.NewGuid();
			var userId2 = Guid.NewGuid();

			// Act
			_tracker.AddConnection(userId1, "conn-1");
			_tracker.AddConnection(userId2, "conn-2");

			// Assert
			CollectionAssert.AreEquivalent(new[] { "conn-1" }, _tracker.GetConnections(userId1));
			CollectionAssert.AreEquivalent(new[] { "conn-2" }, _tracker.GetConnections(userId2));
		}

		[Test]
		public void RemoveConnection_RemovesOnlyTheSpecifiedConnection()
		{
			// Arrange
			var userId = Guid.NewGuid();
			_tracker.AddConnection(userId, "conn-1");
			_tracker.AddConnection(userId, "conn-2");

			// Act
			_tracker.RemoveConnection(userId, "conn-1");

			// Assert
			CollectionAssert.AreEquivalent(new[] { "conn-2" }, _tracker.GetConnections(userId));
		}

		[Test]
		public void RemoveConnection_LastConnectionForUser_UserNoLongerHasConnections()
		{
			// Arrange
			var userId = Guid.NewGuid();
			_tracker.AddConnection(userId, "conn-1");

			// Act
			_tracker.RemoveConnection(userId, "conn-1");

			// Assert
			Assert.IsEmpty(_tracker.GetConnections(userId), "Removing a user's last connection should leave them with no tracked connections.");
		}

		[Test]
		public void RemoveConnection_ForUnknownUser_DoesNotThrow()
		{
			// Act & Assert
			Assert.DoesNotThrow(() => _tracker.RemoveConnection(Guid.NewGuid(), "conn-1"));
		}

		[Test]
		public void RemoveConnection_UnknownConnectionIdForTrackedUser_DoesNotAffectExistingConnections()
		{
			// Arrange
			var userId = Guid.NewGuid();
			_tracker.AddConnection(userId, "conn-1");

			// Act
			_tracker.RemoveConnection(userId, "conn-unknown");

			// Assert
			CollectionAssert.AreEquivalent(new[] { "conn-1" }, _tracker.GetConnections(userId));
		}
	}
}
