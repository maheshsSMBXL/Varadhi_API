using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
    public class SupportFAQSection
    {
        [Key]
        public int FAQSectionId { get; set; }
        public string? FAQSectionName { get; set; }
    }
}
