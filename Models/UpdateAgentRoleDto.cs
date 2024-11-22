namespace Varadhi.Models
{
    public class UpdateAgentRoleDto
    {
        public int TenantId { get; set; }
        public string AgentId { get; set; }
        public int NewRoleId { get; set; }
    }
}
