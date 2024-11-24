namespace Varadhi.Models
{
	public class OTPResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string Error { get; set; } // Optional, for error details
	}
}
