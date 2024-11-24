namespace Varadhi.Models
{
    public class VerifyTenantResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? TenantId { get; set; } // Nullable in case of failure
    }
}
