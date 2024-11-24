namespace Varadhi.Models
{
    public class InviteAgentRequest
    {
        public int TenantId { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
    }
}
