namespace Varadhi.Models
{
	public class AgentCustomerRequest
	{
		public string AgentId { get; set; }
	}

	public class AgentCustomerResponse
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public List<Customer> Customers { get; set; }
		public string Error { get; set; }
	}

	public class Customer
	{
		public string CustomerId { get; set; }
	}
}
