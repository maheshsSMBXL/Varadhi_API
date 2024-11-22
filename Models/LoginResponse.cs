namespace Varadhi.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? TenantId { get; set; } // Nullable in case of login failure
    }
}
