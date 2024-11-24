namespace Varadhi.Models
{
    public class ChangePasswordRequest
    {
        public string VerificationCode { get; set; }
        public string NewPassword { get; set; }
    }
}
