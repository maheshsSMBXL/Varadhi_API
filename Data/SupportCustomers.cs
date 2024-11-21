using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportCustomers
    {
        [Key]
        public string CustomerId { get; set; }
        public int? TenantId { get; set; }
        public string SocketId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
