namespace Varadhi.Models
{
    public class InviteAgentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string InvitationLink { get; set; }  // Optional
    }
}
