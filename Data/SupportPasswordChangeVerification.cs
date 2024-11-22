using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportPasswordChangeVerification
    {
        [Key]
        public int VerificationID { get; set; }
        public int? TenantID { get; set; }
        public string? VerificationCode { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
