namespace Varadhi.Models
{
	public class AssignmentResponse
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public string AgentId { get; set; }
		public string CustomerId { get; set; }
		public string Detail { get; set; } // Optional for error details
	}

}
