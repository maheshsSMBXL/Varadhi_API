namespace Varadhi.Models
{
	public class ChatHistoryRequest
	{
		public string AgentId { get; set; }
		public string CustomerId { get; set; }
	}

	public class ChatHistoryResponse
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public List<ChatMessage> Data { get; set; }
		public string Error { get; set; }
	}

	public class ChatMessage
	{
		public int ChatId { get; set; }
		public string TenantId { get; set; }
		public string AgentId { get; set; }
		public string CustomerId { get; set; }
		public string Message { get; set; }
		public string Sender { get; set; }
		public string FileUrl { get; set; }
		public string FileName { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
