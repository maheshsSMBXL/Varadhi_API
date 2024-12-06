namespace Varadhi.Models
{
	public class ResolveEmailData
	{
		public string From { get; set; }
		public string To { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Type { get; set; }
		public string Bcc { get; set; }
		public string Cc { get; set; }
		public int Ticketid { get; set; }
	}
}
