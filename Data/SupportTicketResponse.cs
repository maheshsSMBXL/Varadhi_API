using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class SupportTicketResponse
	{
		[Key]
		public int ID { get; set; }
		public int TicketId { get; set; }
		public int? TenantId { get; set; }
		public string? CustomerId { get; set; }
		public string? AgentId { get; set; }
		public string? Reply { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
