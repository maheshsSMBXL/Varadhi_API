using System.ComponentModel.DataAnnotations;

namespace Varadhi.Data
{
	public class SupportEmailMessages
	{

		[Key]
		public int Id { get; set; } // Primary key

		public int TicketId { get; set; } // Unique ticket identifier

		public string Sender { get; set; } // Sender's email address

		public string InternetMessageId { get; set; } // Unique identifier for the email

		public string Subject { get; set; } // Email subject

		public string BodyPreview { get; set; } // Short preview of the email body

		public string ConversationId { get; set; } // Graph API conversation ID

		public DateTime ReceivedDateTime { get; set; } // When the email was received

		public DateTime? SentDateTime { get; set; } // When the email was sent

		public bool HasAttachments { get; set; } // Indicates if the email has attachments

		public string Importance { get; set; } // Importance level (e.g., normal, high)

		public bool IsRead { get; set; } // Indicates if the email has been read

		public string ParentFolderId { get; set; } // Folder ID where the email is stored

		public string WebLink { get; set; } // Link to view the email in the mailbox

		public string Content {  get; set; }

		public string EmailMessageId { get; set; }

		public string? RecievedEmail { get; set; }
		public DateTime CreatedDateTime { get; set; } // Record creation timestamp
	}
}
