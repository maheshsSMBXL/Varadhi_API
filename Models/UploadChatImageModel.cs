using System.ComponentModel.DataAnnotations;

namespace Varadhi.Models
{
	public class UploadChatImageModel
	{
		[Required]
		public IFormFile FileUrl { get; set; } // Image in multipart format

		[Required]
		public string TenantId { get; set; }

		[Required]
		public string AgentId { get; set; }

		[Required]
		public string CustomerId { get; set; }

		public string Message { get; set; } = "";

		[Required]
		public string Sender { get; set; }

		public string FileBase64 { get; set; } = "";
		public string FileName { get; set; } = "";
	}
}
