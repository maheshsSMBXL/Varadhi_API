namespace Varadhi.Models
{
    public class VerifyTenantRequest
    {
        public string TenantKey { get; set; }
        public string VerificationCode { get; set; }
    }
}
