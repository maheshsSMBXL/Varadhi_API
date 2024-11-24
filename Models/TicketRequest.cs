namespace Varadhi.Models
{
	public class TicketRequest
	{
		public int Start { get; set; }
		public int End { get; set; }
		public string AssignedTo { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public string Destination { get; set; } = string.Empty;
		public string CustomerId { get; set; } = string.Empty;
		public int TenantId { get; set; } // Add tenantId to the request
	}

	public class TicketResponse
	{
		public int TicketId { get; set; }
		public int TenantId { get; set; }
		public string CustomerId { get; set; }
		public string Complaint { get; set; }
		public string Status { get; set; }
		public string AssignedTo { get; set; }
		public string Comments { get; set; }
		public string Destination { get; set; }
		public DateTime CreatedAt { get; set; }

		public string CustomerName { get; set; }
	}

	public class TicketRequestCustomer
	{
		public string CustomerId { get; set; }
	}
}
