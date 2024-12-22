namespace Varadhi.Models
{
	public class PostCustomerNotesRequest
	{
		public string CustomerName { get; set; }

		public string CustomerEmail { get; set; }

		public string CustomerNumber { get; set; }

		public string CustomerNotes { get; set; }

		public string updatedby { get; set; }

		public DateTime createdDate { get; set; }

		public DateTime updatedDate { get; set; }

		public bool isactive { get; set; }
	}

}
