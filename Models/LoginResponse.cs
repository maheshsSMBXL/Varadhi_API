namespace Varadhi.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
		public string AgentId { get; set; }
		public string TenantId { get; set; }
		public string Status { get; set; }
		public string RoleName { get; set; }
		public string? Token { get; set; } // JWT Token
		public string Error { get; set; } // Optional, for error details
    }
}
