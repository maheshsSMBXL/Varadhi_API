using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportInvitations
    {
        [Key]
        public string InvitationID { get; set; }        
        public string? TenantID { get; set; }
        public int? RoleID { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
