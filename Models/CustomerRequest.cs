using System.ComponentModel.DataAnnotations;

namespace Varadhi.Models
{
	public class CustomerRequest
	{
		[Required]
		public string CustomerId { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Issue { get; set; }

		public string Description { get; set; }

		[Required]
		[MaxLength(45)]
		public string IP { get; set; }

		public string DeviceType { get; set; }

		public string Browser { get; set; }

		public string City { get; set; }

		public string CountryName { get; set; }

		public string CountryCode { get; set; }

		public string Region { get; set; }
	}
}
