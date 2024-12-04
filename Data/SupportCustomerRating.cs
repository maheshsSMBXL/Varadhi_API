namespace Varadhi.Data
{
	public class SupportCustomerRating
	{
		public int Id { get; set; }                  // Primary Key
		public string CustomerId { get; set; }       // Foreign Key or Reference ID
		public string? AgentId { get; set; }          // Reference ID
		public int Rating { get; set; }              // Rating (e.g., 1-5 stars)
		public DateTime CreateDate { get; set; }     // Timestamp of the feedback
		public string? TenantId { get; set; }         // For multi-tenancy support
	}
}
