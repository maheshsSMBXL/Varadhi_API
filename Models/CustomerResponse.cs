namespace Varadhi.Models
{
	public class CustomerResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public CustomerRequest CustomerDetails { get; set; }
	}
}
