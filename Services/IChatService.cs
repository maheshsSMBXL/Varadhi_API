using Varadhi.Models;

namespace Varadhi.Services
{
	public interface IChatService
	{
		Task<ChatResponse> PostChatMessageAsync(ChatRequest data);
	}
}
