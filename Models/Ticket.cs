using Varadhi.Data;

namespace Varadhi.Models
{
	public class TicketResponseByAgentid
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public List<SupportTickets> Tickets { get; set; }

		public Pagination Pagination { get; set; } // New property for pagination
		public string Error { get; set; }
	}

	public class Pagination
	{
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int TotalRecords { get; set; }
		public int TotalPages { get; set; }
	}
}
