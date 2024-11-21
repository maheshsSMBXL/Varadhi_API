using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportAgentWorkHours
    {
        [Key]
        public int WorkId { get; set; }
        public string AgentId { get; set; }
        public int ConnectionsCapacity { get; set; }
        public string PreferredLanguage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
