using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class SupportAgentSignature
	{
		[Key]
		public Guid Id { get; set; }

		public  string AgentId { get;set;}

		public string Signature { get;set;}

		public DateTime createdAt { get; set; }

		public DateTime updatedAt { get; set; }


	}
}
