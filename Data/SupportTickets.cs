using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportTickets
    {
        [Key]
        public int TicketId { get; set; }
        public int? TenantId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string AssignedTo { get; set; }
        public string Complaint { get; set; }
        public string Status { get; set; }
        public string Destination { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Email { get; set; }
        public string Comments { get; set; }
    }
}
