using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net.Sockets;
using Varadhi.Data;
using Varadhi.Models;
using Varadhi.Services;

namespace Varadhi.Controllers
{
	public class EmailTicketingController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;
		private readonly IConfiguration _configuration;
		private readonly SocketIoService _socketIOService;
		public EmailTicketingController(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration, SocketIoService socketIOService)
		{
			_context = context;
			_emailService = emailService;
			_configuration = configuration;
			_socketIOService = socketIOService;

		}

		[HttpPost("ValidconversationId")]
		public async Task<IActionResult> ValidateConversationId([FromBody] ConversationIdrequest request)
		{
			try
			{
				var convid = await _context.SupportEmailMessages.FirstOrDefaultAsync(S => S.ConversationId == request.ConversationId);
				if (convid == null)
				{
					return Ok(new
					{
						success = true,
						isavailable = false
					});
				}
				else
				{
					return Ok(new
					{
						success = true,
						isavailable = true,
						conversationId = request.ConversationId,
						ticketId = convid.TicketId,
					});
				}
			}
			catch (Exception ex)
			{
				// Handle any errors
				return BadRequest(new
				{
					success = false,
					message = "Error occurred during registration.",
					error = ex.Message
				});
			}
		}

		[HttpPost("validNewconversation")]
		public async Task<IActionResult> validNewconversation([FromBody] ValidaTicketidRequest request)
		{
			try
			{
				var tickid = await _context.SupportEmailMessages.FirstOrDefaultAsync(S => S.TicketId == request.TicketId);
				if (tickid == null)
				{
					return Ok(new
					{
						success = true,
						isavailable = false
					});
				}
				else
				{
					return Ok(new
					{
						success = true,
						isavailable = true,
						ticketId = tickid.TicketId,
					});
				}
			}
			catch (Exception ex)
			{
				// Handle any errors
				return BadRequest(new
				{
					success = false,
					message = "Error occurred during registration.",
					error = ex.Message
				});
			}
		}

		[HttpPost("PostEmailMessageData")]
		public async Task<IActionResult> PostEmailMessageData([FromBody] PostEmailMessageDataRequest request)
		{
			try
			{
				var EmailMessage = new SupportEmailMessages
				{
					BodyPreview = request.BodyPreview,
					ConversationId = request.ConversationId,
					CreatedDateTime = DateTime.UtcNow,
					HasAttachments = request.HasAttachments,
					Importance = request.Importance,
					InternetMessageId = request.InternetMessageId,
					IsRead = request.IsRead,
					TicketId = request.TicketId,
					ParentFolderId = request.ParentFolderId,
					ReceivedDateTime = DateTime.UtcNow,
					Sender = request.Sender,
					SentDateTime = request.SentDateTime,
					Subject = request.Subject,
					WebLink = request.WebLink,
					Content = request.Content,
					EmailMessageId = request.EmailMessageId,

				};
				await _context.SupportEmailMessages.AddAsync(EmailMessage);
				_context.SaveChanges();
				var assignedAgent = await _context.SupportTickets.Where(em => em.TicketId == request.TicketId).Select(em => new
				{
					AssignedTo = em.AssignedTo
				}).FirstOrDefaultAsync();
				var eventData = new
				{
					TicketId = EmailMessage.TicketId,
					AgentId = assignedAgent,
					Message = "New Message From Ticket",


				};
				if (request.Sender == "Customer")
				{
					if(assignedAgent.AssignedTo != "Unassigned" )
					{
						await _socketIOService.EmitEventAsync("NewEmailMessage", eventData);
					}
					
				}
				
				return Ok(
					new
					{
						success = true,
						message = "your email message is recorded successfully"
					});
			}
			catch (Exception ex)
			{
				// Handle any errors
				return BadRequest(new
				{
					success = false,
					message = "Error occurred during registration.",
					error = ex.Message
				});
			}
		}

		[HttpPost("PostInternalNotes")]
		public async Task<IActionResult> PostInternalNotes([FromBody] PostInternalNotesRequest request)
		{
			try
			{
				var internalNote = new SupportInternalNotes
				{
					AgentId = request.AgentId,
					createdDate = DateTime.UtcNow,
					InternalNotes = request.InternalNotes,
					TicketId = request.TicketId,
					Isactive = true
				};
				await _context.SupportInternalNotes.AddAsync(internalNote);
				_context.SaveChanges();
				return Ok(
					new
					{
						success = true,
						message = "your internalnote created successfully"
					});
			}
			catch (Exception ex)
			{
				// Handle any errors
				return BadRequest(new
				{
					success = false,
					message = "Error occurred during internal note.",
					error = ex.Message
				});
			}
		}

		//[HttpPost("PostCustomerNotes")]
		//public async Task<IActionResult> PostCustomerNotes([FromBody] PostCustomerNotesRequest request)
		//{
		//	try
		//	{
		//		var existingNote = await _context.SupportCustomerNotes.FirstOrDefaultAsync(sc => sc.CustomerEmail == request.CustomerEmail);

		//		if (existingNote == null)
		//		{
		//			var customerNote = new SupportCustomerNotes
		//			{
		//				CustomerNotes = request.CustomerNotes,
		//				createdDate = DateTime.UtcNow,
		//				CustomerEmail = request.CustomerEmail,
		//				CustomerName = request.CustomerName,
		//				CustomerNumber = request.CustomerNumber,
		//				updatedby = request.updatedby,
		//				isactive = true,

		//			};
		//			await _context.SupportCustomerNotes.AddAsync(customerNote);
		//			_context.SaveChanges();

		//		}
		//		else
		//		{
		//			// If existing note found, update its properties with the latest data
		//			existingNote.CustomerNotes = request.CustomerNotes;
		//			existingNote.updatedDate = DateTime.UtcNow; // Assuming you have an UpdatedDate field
		//			existingNote.CustomerName = request.CustomerName;
		//			existingNote.CustomerNumber = request.CustomerNumber;
		//			existingNote.updatedby = request.updatedby;
		//			existingNote.isactive = true; // If this field is part of the request

		//			// Optionally, you can update the CustomerEmail if it's allowed and necessary
		//			// existingNote.CustomerEmail = request.CustomerEmail;

		//			// Mark the entity as modified
		//			await _context.SupportCustomerNotes.ExecuteUpdateAsync(existingNote);

		//		}

		//		return Ok(
		//			new
		//			{
		//				success = true,
		//				message = "your internalnote created successfully"
		//			});
		//	}
		//	catch (Exception ex)
		//	{
		//		// Handle any errors
		//		return BadRequest(new
		//		{
		//			success = false,
		//			message = "Error occurred during internal note.",
		//			error = ex.Message
		//		});
		//	}
		//}

		[HttpPost("PostCustomerNotes")]
		public async Task<IActionResult> PostCustomerNotes([FromBody] PostCustomerNotesRequest request)
		{
			try
			{
				if (request == null || string.IsNullOrWhiteSpace(request.CustomerEmail))
				{
					return BadRequest(new
					{
						success = false,
						message = "Invalid request data. Please provide all required fields."
					});
				}

				var existingNote = await _context.SupportCustomerNotes
					.FirstOrDefaultAsync(sc => sc.CustomerEmail == request.CustomerEmail);

				if (existingNote == null)
				{
					// Add new customer note
					var customerNote = new SupportCustomerNotes
					{
						CustomerNotes = request.CustomerNotes,
						createdDate = DateTime.UtcNow,
						CustomerEmail = request.CustomerEmail,
						CustomerName = request.CustomerName,
						CustomerNumber = request.CustomerNumber,
						updatedby = request.updatedby,
						isactive = true
					};

					await _context.SupportCustomerNotes.AddAsync(customerNote);
					await _context.SaveChangesAsync();
				}
				else
				{
					// Update existing customer note
					existingNote.CustomerNotes = request.CustomerNotes;
					existingNote.updatedDate = DateTime.UtcNow;
					existingNote.CustomerName = request.CustomerName;
					existingNote.CustomerNumber = request.CustomerNumber;
					existingNote.updatedby = request.updatedby;
					existingNote.isactive = true;

					_context.SupportCustomerNotes.Update(existingNote);
					await _context.SaveChangesAsync();
				}

				return Ok(new
				{
					success = true,
					message = "Customer note saved successfully."
				});
			}
			catch (Exception ex)
			{
				// Log the exception if logging is available
				return StatusCode(500, new
				{
					success = false,
					message = "An error occurred while processing the request.",
					error = ex.Message
				});
			}
		}


		//get email conversations
		//pass the ticket id and get the data from email message data additionally add the internal notes also get renderd
		/*
		 select * from emailmessage where ticketid ='234'
		select * from internalnotes where ticketid ='234'
		here i need to write a post api such that when we pass a ticketid it will give me like a list of emailmessage on ticketid combined with internal notes order by date desc

		*/
		//[HttpPost("GetSupportActivities")]
		//public async Task<IActionResult> GetSupportActivities([FromBody] TicketRequestDto request)
		//{
		//	if (request == null || request.TicketId <= 0)
		//	{
		//		return BadRequest("Invalid ticket ID.");
		//	}

		//	// Fetch email messages
		//	var emailMessagesTask = await _context.SupportEmailMessages
		//		.Where(em => em.TicketId == request.TicketId)
		//		.Select(em => new SupportActivityDto
		//		{
		//			Id = em.Id,
		//			TicketId = em.TicketId,
		//			Type = "Email",
		//			Sender = em.Sender,
		//			Subject = em.Subject,
		//			BodyPreview = em.BodyPreview,
		//			Date = em.ReceivedDateTime
		//		})
		//		.ToListAsync();

		//	// Fetch internal notes
		//	var internalNotesTask = await _context.SupportInternalNotes
		//		.Where(inote => inote.TicketId == request.TicketId && inote.Isactive)
		//		.Select(inote => new SupportActivityDto
		//		{
		//			Id = inote.Id,
		//			TicketId = inote.TicketId,
		//			Type = "InternalNote",
		//			InternalNotes = inote.InternalNotes,
		//			AgentId = inote.AgentId,
		//			Date = inote.createdDate
		//		})
		//		.ToListAsync();

		//	// Await both tasks
		//	//await Task(emailMessagesTask, internalNotesTask);

		//	// Combine the lists
		//	var combinedList = emailMessagesTask
		//	.Concat(internalNotesTask)
		//	.OrderByDescending(activity => activity.Date)
		//	.ToList();

		//	return Ok(combinedList);
		//}

		[HttpPost("GetSupportActivities")]
		public async Task<IActionResult> GetSupportActivities([FromBody] TicketRequestDto request)
		{
			if (request == null || request.TicketId <= 0)
			{
				return BadRequest(new { success = false, message = "Invalid ticket ID." });
			}

			// Initialize a list to hold all activities
			var combinedList = new List<SupportActivityDto>();

			// Fetch email messages
			var emailMessages = await _context.SupportEmailMessages
				.Where(em => em.TicketId == request.TicketId)
				.Select(em => new SupportActivityDto
				{
					Id = em.Id,
					TicketId = em.TicketId,
					Type = "Email",
					Sender = em.Sender,
					Subject = em.Subject,
					BodyPreview = em.Content,
					Date = em.ReceivedDateTime
				})
				.ToListAsync();

			combinedList.AddRange(emailMessages);

			// Fetch internal notes
			var internalNotes = await _context.SupportInternalNotes
				.Where(inote => inote.TicketId == request.TicketId && inote.Isactive)
				.Select(inote => new SupportActivityDto
				{
					Id = inote.Id,
					TicketId = inote.TicketId,
					Type = "InternalNote",
					InternalNotes = inote.InternalNotes,
					AgentId = inote.AgentId,
					Date = inote.createdDate 
				})
				.ToListAsync();

			combinedList.AddRange(internalNotes);

			// Additional data based on the ticket type
			if (request.type.Equals("offline", StringComparison.OrdinalIgnoreCase))
			{
				// Fetch ticket description from SupportTickets
				var ticketDescription = await _context.SupportTickets
					.Where(t => t.TicketId == request.TicketId)
                    //c => new {t.Complaint, t.CreatedAt }
                    .Select(t => new { t.Complaint, t.CreatedAt })
					.FirstOrDefaultAsync();
				var detaileDescription = await _context.SupportTickets
					.Where(t => t.TicketId == request.TicketId)
					.Select(t => t.Comments)
					.FirstOrDefaultAsync();
				if (!string.IsNullOrEmpty(ticketDescription.Complaint))
				{
					combinedList.Add(new SupportActivityDto
					{
						Id = request.TicketId, // Using TicketId as Id for description
						TicketId = request.TicketId,
						Type = "TicketDescription",
						Description = ticketDescription.Complaint,
						comments = detaileDescription,
						Date = (DateTime)ticketDescription.CreatedAt// Adjust as needed
                    });
				}
			}
			else if (request.type.Equals("online", StringComparison.OrdinalIgnoreCase))
			{
				// Fetch assigned agent's name
				var assignedAgentId = await _context.SupportTickets
					.Where(t => t.TicketId == request.TicketId)
					.Select(t => t.AssignedTo)
					.FirstOrDefaultAsync();
				var customerId = await _context.SupportTickets
					.Where(t => t.TicketId == request.TicketId)
					.Select(t => t.CustomerId)
					.FirstOrDefaultAsync();

				string? agentName = null;
				if (!string.IsNullOrEmpty(assignedAgentId))
				{
					agentName = await _context.SupportAgents
						.Where(sa => sa.AgentId == assignedAgentId)
						.Select(sa => sa.Name)
						.FirstOrDefaultAsync();
				}

				if (!string.IsNullOrEmpty(agentName))
				{
					combinedList.Add(new SupportActivityDto
					{
						Id = 0, // No specific ID for agent entry
						TicketId = request.TicketId,
						Type = "Agent",
						Sender = agentName,
						//Date = DateTime.UtcNow // Adjust as needed
					});
				}

				// Fetch customer chats
				var customerChats = await _context.SupportChats
					.Where(chat => chat.CustomerId == customerId && chat.AgentId == assignedAgentId)
					.Select(chat => new SupportActivityDto
					{
						Id = chat.ChatId,
						TicketId = request.TicketId,
						Type = "Chat",
						Sender = chat.Sender,
						Message = chat.Message,
						Date = chat.CreatedAt ?? DateTime.UtcNow
					})
					.ToListAsync();

				combinedList.AddRange(customerChats);
			}

			// Order the combined list by date descending
			var orderedList = combinedList
				.OrderByDescending(activity => activity.Date)
				.ToList();

			return Ok(new { success = true, activities = orderedList });
		}

		[HttpPost("updateTicketDetails")]
		public async Task<IActionResult> UpdateTicketDetails([FromBody] UpdateTicketDto request)
		{
			// Validate the incoming request
			if (request == null || request.Ticketid <= 0)
			{
				return BadRequest(new { success = false, message = "Invalid ticket ID." });
			}

			// Fetch the ticket details from the database
			var ticketDetails = await _context.SupportTickets.FirstOrDefaultAsync(r => r.TicketId == request.Ticketid);

			if (ticketDetails == null)
			{
				return BadRequest(new { success = false, message = "Ticket ID not found." });
			}

			// Update fields only if they are provided in the request
			if (!string.IsNullOrWhiteSpace(request.subject))
			{
				ticketDetails.Complaint = request.subject;
			}

			if (!string.IsNullOrWhiteSpace(request.email))
			{
				ticketDetails.Email = request.email;
			}

            if (!string.IsNullOrWhiteSpace(request.destination))
            {
                ticketDetails.Destination = request.destination;
                var eventData = new
                {
                    TicketId = ticketDetails.TicketId,
                    Message = "new Ticket arrived",
                    Mail = ticketDetails.Email


                };
                await _socketIOService.EmitEventAsync("NewTicketArrival", eventData);
            }

            // Save changes to the database
            _context.SupportTickets.Update(ticketDetails);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Ticket details updated successfully." });
		}


		[HttpPost("GetLatestCustomerEmailMessageId")]
		public async Task<IActionResult> GetLatestCustomerEmailMessageId([FromBody] TicketRequestDto request)
		{
			if (request == null || request.TicketId <= 0)
			{
				return BadRequest("Invalid ticket ID.");
			}

			// Define what constitutes a "customer" sender.
			// This could be based on a specific domain, a list of customer emails, etc.
			// For this example, let's assume customer emails end with "@customer.com"

			string customerDomain = "@customer.com";

			var latestEmail = await _context.SupportEmailMessages
				.Where(em => em.TicketId == request.TicketId && em.Sender == "customer")
				.OrderByDescending(em => em.ReceivedDateTime)
				.Select(em => new LatestEmailMessageDto
				{
					EmailMessageId = em.EmailMessageId,
					ReceivedDateTime = em.ReceivedDateTime
				})
				.FirstOrDefaultAsync();

			if (latestEmail == null)
			{
				return NotFound(new
				{
					success = true,
					isavaialble=false,
					message = "No ticketid is available"
				});
			}

			return Ok(latestEmail);
		}


		[HttpPost("GetCustomerinfoByTicketId")]
		public async Task<IActionResult> GetCustomerinfoByTicketId([FromBody] GetCustomerInfoDto request)
		{
			// Validate the ticket ID
			if (request.TicketId <= 0)
			{
				return BadRequest(new { success = false, message = "Invalid Ticket ID." });
			}

			// Fetch the ticket details from SupportTickets
			var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == request.TicketId);

			if (ticket == null)
			{
				return NotFound(new { success = false, message = "Ticket not found." });
			}

			// Extract the email from the ticket
			string? email = ticket.Email;

			// Fetch customer details and notes based on the email
			var customerDetail = await _context.CustomerDetailInfo.FirstOrDefaultAsync(c => c.Email == email);
			var customerNotes = await _context.SupportCustomerNotes.Where(n => n.CustomerEmail == email).ToListAsync();

			// Prepare the response
			var response = new
			{
				TicketId = ticket.TicketId,
				Email = email,
				Name = customerDetail?.Name,
				IP = customerDetail?.IP,
				State = customerDetail?.Region,
				Country = customerDetail?.CountryCode,
				CustomerNotes = customerNotes.Any() ? customerNotes.Select(n => n.CustomerNotes) : null,
				LastTicketDate = ticket.CreatedAt,
				CustomerId=customerDetail.CustomerId
				
			};

			return Ok(response);
		}

		[HttpPost("getTicketsByEmail")]
		public async Task<IActionResult> GetTicketsByEmail([FromBody] GetCustomerInfoDto request)
		{
			// Validate the ticket ID
			if (request.TicketId <= 0)
			{
				return BadRequest(new { success = false, message = "Invalid Ticket ID." });
			}

			// Fetch the ticket to retrieve the email
			var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == request.TicketId);

			if (ticket == null)
			{
				return NotFound(new { success = false, message = "Ticket not found." });
			}

			// Extract the email from the ticket
			string? email = ticket.Email;

			if (string.IsNullOrEmpty(email))
			{
				return NotFound(new { success = false, message = "Email not found for the provided Ticket ID." });
			}

			// Fetch all tickets associated with the email
			var tickets = await _context.SupportTickets
										.Where(t => t.Email == email)
										.Select(t => new
										{
											TicketId = t.TicketId,
											EmailId = t.Email,
											Complaint = t.Complaint,
											Comments = t.Comments,
											CreatedDate = t.CreatedAt,
											Status = t.Status
										})
										.ToListAsync();

			// Check if any tickets exist for the email
			if (!tickets.Any())
			{
				return NotFound(new { success = false, message = "No tickets found for the provided email." });
			}

			// Return the tickets
			return Ok(new
			{
				success = true,
				email = email,
				ticketCount = tickets.Count,
				tickets = tickets
			});
		}


		[HttpPost("getTicketsByEmailId")]

		public async Task<IActionResult> GetTicketsByEmailId([FromBody] GetTicketsByEmailIdDto request)

		{

			// Validate the email address

			if (string.IsNullOrEmpty(request.Email))

			{

				return BadRequest(new { success = false, message = "Invalid email address." });

			}

			// Fetch all tickets associated with the email

			var tickets = await _context.SupportTickets
							.Where(t => t.Email == request.Email)
							.Select(t => new
							{
								TicketId = t.TicketId,
								EmailId = t.Email,
								Complaint = t.Complaint,
								Comments = t.Comments,
								CreatedDate = t.CreatedAt,
								Status = t.Status
							})
							.ToListAsync();


			return Ok(new

			{

				success = true,

				email = request.Email,

				ticketCount = tickets.Count,

				tickets = tickets

			});

		}

		[HttpPost("validateDuplicateTicket")]


		public async Task<IActionResult> ValidateDuplicateTicket([FromBody] ValidateDuplicateTktDto request)

		{

			// Validate the email address

			if (string.IsNullOrEmpty(request.customerId))

			{

				return BadRequest(new { success = false, message = "Invalid CustomerID" });

			}

			// Fetch all tickets associated with the email

			var tickets = await _context.SupportTickets
							.Where(t => t.CustomerId == request.customerId)
							.Select(t => new
							{
								TicketId = t.TicketId,
								EmailId = t.Email,
								Complaint = t.Complaint,
								Comments = t.Comments,
								CreatedDate = t.CreatedAt,
								Status = t.Status
							})
							.ToListAsync();

			if(tickets.Count == 0)
			{
				return Ok(new

				{

					success = true,
					isavailable = false,
					message = "Ticket is not Available",

				});
			}

			return Ok(new

			{

				success = true,
				isavailable = true,
				email = tickets[0].TicketId,

				message ="Ticket Already Available",

			});

		}



		[HttpPost("CheckInReplyTo")]


		public async Task<IActionResult> CheckInReplyTo([FromBody] CheckInReplyToDto request)

		{

			// Validate the email address

			if (string.IsNullOrEmpty(request.inReplyTo))

			{

				return BadRequest(new { success = false, message = "Invalid CustomerID" });

			}

			// Fetch all tickets associated with the email

			var tickets = await _context.SupportEmailMessages
							.Where(t => t.InternetMessageId == request.inReplyTo)
							.Select(t => new
							{
								messageId = t.EmailMessageId,
								ticketid = t.TicketId,
							})
							.ToListAsync();

			if (tickets.Count == 0)
			{
				return Ok(new

				{

					success = true,
					isavailable = false,
					message = "Reply mail is not available",

				});
			}

			return Ok(new

			{

				success = true,
				isavailable = true,
				ticketid = tickets[0].ticketid,

				message = "Ticket Already Available",

			});

		}




        [HttpPost("UpdateCustomerName")]


        public async Task<IActionResult> UpdateCustomerName([FromBody] UpdateCustomerNameDto request)

        {

            // Validate the email address

            if (string.IsNullOrEmpty(request.CustomerId))

            {

                return BadRequest(new { success = false, message = "Invalid CustomerID" });

            }

            // Fetch all tickets associated with the email
            var customerDetail=await _context.CustomerDetailInfo.FirstOrDefaultAsync(t=>t.CustomerId==request.CustomerId);
            if (customerDetail == null)
            {
                return BadRequest(new { success = false, message = "customerDetails not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                customerDetail.Name = request.Name;
            }

            _context.CustomerDetailInfo.Update(customerDetail);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Customer name updated successfully." });

        }


        [HttpPost("GetCustomerByTenantId")]
        public async Task<IActionResult> GetCustomerInfoByTenantId([FromBody] GetCustomerInfoByTenantIdDto request)
        {
            try
            {
                // Validate the input
                if (string.IsNullOrWhiteSpace(request.Input))
                {
                    return BadRequest(new { success = false, message = "Input cannot be null or empty." });
                }

                // Search for customer details
                var results = await _context.CustomerDetailInfo
                    .Where(c => c.Name.Contains(request.Input) || c.Email.Contains(request.Input))
                    .Select(c => new { c.CustomerId, c.Name, c.Email })
                    .ToListAsync();

                // Check if results are empty
                if (!results.Any())
                {
                    return NotFound(new { success = false, message = "No customers found matching the input criteria." });
                }

                // Return the results
                return Ok(new { success = true, data = results });
            }
            catch (Exception ex)
            {


                // Return a generic error response
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An unexpected error occurred. Please try again later."
                });
            }
        }

        //validate email for customercreation
        [HttpPost("ValidateEmailCustomer")]
        public async Task<IActionResult> ValidateEmailCustomer([FromBody] ValidateEmailCustomerDto request)
        {
            try
            {
                // Validate the input
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Input cannot be null or empty." });
                }

                // Search for customer details
                var results = await _context.CustomerDetailInfo
                    .Where(c => c.Email.Contains(request.Email))
                    .Select(c => new { c.CustomerId, c.Name, c.Email })
                    .ToListAsync();

                // Check if results are empty
                if (!results.Any())
                {
                    return Ok(new { success = true,isavailable = false, message = "No customers found matching the input criteria." });
                }

                // Return the results
                return Ok(new { success = true, isavailable = true, message = "Email ALready Exsist" });
            }
            catch (Exception ex)
            {


                // Return a generic error response
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An unexpected error occurred. Please try again later."
                });
            }
        }



        // DTO for request
        public class TicketRequestDto
		{
			public int TicketId { get; set; }

			public string? type { get; set; }
		}
		//public class SupportActivityDto
		//{
		//	public int Id { get; set; }
		//	public int TicketId { get; set; }
		//	public string Type { get; set; } // "Email" or "InternalNote"
		//	public string Sender { get; set; } // For Emails
		//	public string Subject { get; set; } // For Emails
		//	public string BodyPreview { get; set; } // For Emails
		//	public string InternalNotes { get; set; } // For Internal Notes
		//	public string AgentId { get; set; } // For Internal Notes
		//	public DateTime Date { get; set; } // CreatedDateTime or createdDate
		//}
		public class SupportActivityDto
		{
			public int Id { get; set; }
			public int TicketId { get; set; }
			public string Type { get; set; } // "Email", "InternalNote", "TicketDescription", "Chat"
			public string? Sender { get; set; }
			public string? Subject { get; set; }
			public string? BodyPreview { get; set; }
			public string? InternalNotes { get; set; }
			public string? AgentId { get; set; }
			public string? Message { get; set; }
			public string? comments { get; set; }
			public DateTime Date { get; set; }
			public string? Description { get; set; } // For "offline" type
		}


	}
}

