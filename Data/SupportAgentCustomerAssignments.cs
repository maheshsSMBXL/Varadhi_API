using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportAgentCustomerAssignments
    {
        [Key]
        public int Id { get; set; }
        public string? AgentId { get; set; }
        public string? CustomerId { get; set; }
        public int? TenantId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? TerminatedAt { get; set; }
    }
}
