using System.ComponentModel.DataAnnotations;

namespace Varadhi.Models
{
	public class ForgotPasswordRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}

	public class ValidateOtpRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Otp { get; set; }
	}

	public class ResetPasswordRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string NewPassword { get; set; }
	}

}
