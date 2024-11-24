namespace Varadhi.Models
{
    public class DecryptInvitationResponse
    {
        public bool Success { get; set; }
        public string TenantId { get; set; }
        public string TenantKey { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string Message { get; set; }
    }
}
