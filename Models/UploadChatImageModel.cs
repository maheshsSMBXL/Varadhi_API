namespace Varadhi.Models
{
	public class UploadChatImageModel
	{
		public List<IFormFile> ImageFiles { get; set; }

			// This will hold the JSON string of SupportChats
			public string Request { get; set; }

	}


}
