namespace Varadhi.Models
{
	public class UpdateTicketRequest
	{
		public int TicketId { get; set; }
		public string NewStatus { get; set; }
	}

	public class UpdateTicketResponse
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public int? TicketId { get; set; }
		public string NewStatus { get; set; }
		public string Error { get; set; }
	}
}
