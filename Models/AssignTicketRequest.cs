namespace Varadhi.Models
{
	public class AssignTicketRequest
	{
		public int TicketId { get; set; }
		public string AgentId { get; set; }
	}
	public class AssignTicketResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int TicketId { get; set; }
		public string AssignedTo { get; set; }
		public string Error { get; set; }
	}
}
