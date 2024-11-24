namespace Varadhi.Models
{
	public class AgentRequest
	{
		public string TenantId { get; set; }
	}

	public class AgentResponse
	{
		public bool Success { get; set; }
		public List<Agent> Agents { get; set; }
		public string Message { get; set; }
		public string Error { get; set; }
	}
	public class AgentResponseById
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public Agent Agent { get; set; }
		public string Error { get; set; }
	}

	public class Agent
	{
		public string AgentId { get; set; }
		public string AgentName { get; set; }
		public string AgentEmail { get; set; }
	}
}
