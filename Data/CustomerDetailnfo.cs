using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class CustomerDetailInfo
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string CustomerId { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		public string Issue { get; set; }

		public string Description { get; set; }

		[Required]
		[MaxLength(45)] // Typical length for IPv4 and IPv6 addresses
		public string IP { get; set; }

		public string DeviceType { get; set; }

		public string Browser { get; set; }

		public string City { get; set; }

		public string CountryName { get; set; }

		public string CountryCode { get; set; }

		public string Region { get; set; }
		// Date when the record was created
		
		public DateTime DateCreated { get; set; }

		// Date when the record was last updated
		public DateTime? DateUpdated { get; set; }
	}
}
