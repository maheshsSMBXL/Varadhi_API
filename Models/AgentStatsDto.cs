namespace Varadhi.Models
{
	public class AgentStatsDto
	{
		public string AgentId { get; set; }
		public int TotalConnectedCustomers { get; set; }
		public int TotalPossibleRatingPoints { get; set; }
		public int TotalRatingPointsReceived { get; set; }
		public double CustomerSatisfactionRatePercentage { get; set; }
		public int OpenTickets { get; set; }
		public int ClosedTickets { get; set; }
	}
}
