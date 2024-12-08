using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class SupportAgentForgotPwdVerification
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string VerificationCode { get; set; }

		public bool IsVerified { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? ExpiredAt { get; set; }
	}
}
