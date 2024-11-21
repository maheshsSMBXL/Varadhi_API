using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportAgents
    {
        [Key]
        public string AgentId { get; set; }
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string SocketId { get; set; }
        public string Status { get; set; }
        public bool? Busy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
