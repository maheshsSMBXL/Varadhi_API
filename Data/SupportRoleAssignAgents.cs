using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportRoleAssignAgents
    {
        [Key]
        public int AssignmentID { get; set; }
        public string? AgentID { get; set; }
        public int? RoleID { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}
