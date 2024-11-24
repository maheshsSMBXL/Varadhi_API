namespace Varadhi.Models
{
	public class RaiseTicketRequest
	{
		public int TenantId { get; set; }
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string Email { get; set; }
		public string Complaint { get; set; }
		public string Comments { get; set; }
		public string Status { get; set; } = "open";
		public string AssignedTo { get; set; } = "Unassigned";
		public string Destination { get; set; } = "online";
	}
}
