using System.Linq.Expressions;
using Comms_Server.DTOs.Message;

namespace Comms_Server.Shared
{
	public static class Projections
	{
		public static readonly Expression<Func<Database.Models.Message, MessageDto>> ToMessageDto = m => new MessageDto
		{
			Id = m.Id,
			ConversationId = m.ConversationId,
			SenderId = m.SenderId,
			SenderUsername = m.Sender.UserName!,
			Content = m.Content,
			SentAt = m.SentAt
		};
	}
}
