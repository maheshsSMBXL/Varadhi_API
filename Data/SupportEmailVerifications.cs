using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportEmailVerifications
    {
        [Key]
        public int Id { get; set; }
        public string AgentId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
