using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportAdminsList
    {
        [Key]
        public int AdminId { get; set; }
        public string? Email { get; set; }
    }
}
