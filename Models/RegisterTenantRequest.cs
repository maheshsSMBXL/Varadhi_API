namespace Varadhi.Models
{
    public class RegisterTenantRequest
    {
        public string CompanyName { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public string CompanySize { get; set; }
    }
}
