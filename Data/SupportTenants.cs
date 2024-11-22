using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportTenants
    {
        [Key]
        public int TenantId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? TenantKey { get; set; }
        public string? AdminEmail { get; set; }
        public bool? IsVerified { get; set; }
        public string? VerificationCode { get; set; }
        public string? CompanySize { get; set; }
    }
}
