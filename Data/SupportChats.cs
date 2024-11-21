using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportChats
    {
        [Key]
        public int ChatId { get; set; }
        public string TenantId { get; set; }
        public string AgentId { get; set; }
        public string CustomerId { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
