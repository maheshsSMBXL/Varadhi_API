using Varadhi.Data;

namespace Varadhi.Models
{
    public class TicketDetailsResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public SupportTickets Ticket { get; set; }
        public string Error { get; set; }
    }
}
