using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportTenantPassword
    {
        [Key]
        public int PasswordId { get; set; }
        public int TenantId { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
