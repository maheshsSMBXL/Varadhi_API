using Varadhi.Data;

namespace Varadhi.Models
{
	public class TicketResponseByAgentid
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public List<SupportTickets> Tickets { get; set; }
		public string Error { get; set; }
	}
}
