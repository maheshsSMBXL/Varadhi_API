using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class SupportInternalNotes
	{
		[Key]
		public int Id { get; set; }

		public int TicketId { get; set; }
		public string InternalNotes { get; set; }

		public string AgentId { get; set; }

		public DateTime createdDate { get; set; }

		public bool Isactive { get; set; }

	}
}
