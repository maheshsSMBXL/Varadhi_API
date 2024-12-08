using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class PasswordChangeLog
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string AgentId { get; set; }

		[Required]
		public DateTime ChangedAt { get; set; }
		public string ChangedBy { get; set; } // e.g., "User" or "Admin"

		public string Remarks { get; set; }
	}
}
