namespace Varadhi.Models
{
	public class ChatRequest
	{
		public string TenantId { get; set; }
		public string AgentId { get; set; }
		public string CustomerId { get; set; }
		public string Message { get; set; } = "";
		public string Sender { get; set; }
		public string FileBase64 { get; set; } = "";
		public string FileName { get; set; } = "";
	}
}
