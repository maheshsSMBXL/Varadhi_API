namespace Varadhi.Models
{
    public class RegisterAgentRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int TenantId { get; set; }
        public int RoleId { get; set; }
    }
}
