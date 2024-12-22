namespace Varadhi.Models
{
	public class PostInternalNotesRequest
	{
		public int TicketId { get; set; }
		public string InternalNotes { get; set; }

		public string AgentId { get; set; }

		public DateTime createdDate { get; set; }

		public bool? Isactive { get; set; }
	}
}
