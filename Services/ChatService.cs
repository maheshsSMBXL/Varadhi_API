using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Varadhi.Data;
using Varadhi.Models;
namespace Varadhi.Services
{
	public class ChatService : IChatService
	{
		private readonly ApplicationDbContext _context;
		

		public ChatService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<ChatResponse> PostChatMessageAsync(ChatRequest data)
		{
			try
			{
				string fileUrl = "";
				if (!string.IsNullOrEmpty(data.FileBase64))
				{
					var directory = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "chat_images");

					// Create directory if it doesn't exist
					if (!Directory.Exists(directory))
						Directory.CreateDirectory(directory);

					// Generate a unique file name
					string uniqueFileName = $"{Guid.NewGuid()}_{data.FileName}";
					string filePath = Path.Combine(directory, uniqueFileName);

					// Convert base64 string to byte array and save the file
					byte[] fileData = Convert.FromBase64String(data.FileBase64);
					await File.WriteAllBytesAsync(filePath, fileData);

					// Generate file URL
					fileUrl = $"/uploads/chat_images/{uniqueFileName}";
				}

				// Insert chat message into the database
				var chatMessage = new SupportChats
				{
					TenantId = data.TenantId,
					AgentId = data.AgentId,
					CustomerId = data.CustomerId,
					Message = data.Message,
					Sender = data.Sender,
					FileUrl = fileUrl,
					FileName = data.FileName,
					CreatedAt = DateTime.Now
				};

				_context.SupportChats.Add(chatMessage);
				await _context.SaveChangesAsync();

				return new ChatResponse { Success = true, Message = "Chat message and file uploaded successfully.", FileUrl = fileUrl };
			}
			catch (Exception ex)
			{
				return new ChatResponse { Success = false, Message = "An error occurred while posting the chat message.", Error = ex.Message };
			}
		}
	}
}
