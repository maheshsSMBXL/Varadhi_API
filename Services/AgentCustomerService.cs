﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Varadhi.Data;
using Varadhi.Models;
using Varadhi.Data;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Varadhi.Services
{
	public class AgentCustomerService : IAgentCustomerService
	{
		private readonly ApplicationDbContext _context;
		private readonly JwtSettings _jwtSettings;
		public AgentCustomerService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
		{
			_context = context;
			_jwtSettings = jwtSettings.Value;
		}

		public async Task<AssignmentResponse> AssignCustomerToAgentAsync(AssignmentRequest request)
		{
			if (request == null || string.IsNullOrEmpty(request.AgentId) || string.IsNullOrEmpty(request.CustomerId) || request.TenantId <= 0)
			{
				return new AssignmentResponse
				{
					Status = "error",
					Message = "Invalid input data."
				};
			}

			try
			{
				// Check if the customer is already assigned to the agent
				var existingAssignment = await _context.SupportAgentCustomerAssignment
					.FirstOrDefaultAsync(a =>
						a.AgentId == request.AgentId &&
						a.CustomerId == request.CustomerId &&
						a.TenantId == request.TenantId &&
						a.TerminatedAt == null);

				//if (existingAssignment != null)
				//{
				//	return new AssignmentResponse
				//	{
				//		Status = "error",
				//		Message = "Customer is already assigned to an agent."
				//	};
				//}

				// Create a new assignment
				var newAssignment = new SupportAgentCustomerAssignments
				{
					AgentId = request.AgentId,
					CustomerId = request.CustomerId,
					TenantId = request.TenantId,
					AssignedAt = DateTime.UtcNow
				};

				await _context.SupportAgentCustomerAssignment.AddAsync(newAssignment);
				await _context.SaveChangesAsync();

				return new AssignmentResponse
				{
					Status = "success",
					Message = "Customer assigned to agent successfully.",
					AgentId = request.AgentId,
					CustomerId = request.CustomerId
				};
			}
			catch (Exception ex)
			{
				return new AssignmentResponse
				{
					Status = "error",
					Message = "Failed to assign customer to agent.",
					Detail = ex.Message
				};
			}
		}
		public async Task<OTPResponse> ValidateOTPAsync(OTPRequest data)
		{
			try
			{
				// Check if agentId and verification code match
				var verificationRecord = await _context.SupportEmailVerifications
					.FirstOrDefaultAsync(v => v.AgentId == data.AgentId && v.Token == data.VerificationCode);

				if (verificationRecord == null)
				{
					return new OTPResponse
					{
						Success = false,
						Message = "Invalid OTP. Please try again."
					};
				}

				// Check if already verified
				if ((bool)verificationRecord.IsVerified)
				{
					return new OTPResponse
					{
						Success = false,
						Message = "OTP already verified."
					};
				}

				// Mark as verified
				verificationRecord.IsVerified = true;
				_context.SupportEmailVerifications.Update(verificationRecord);
				await _context.SaveChangesAsync();

				return new OTPResponse
				{
					Success = true,
					Message = "OTP verified successfully."
				};
			}
			catch (Exception ex)
			{
				return new OTPResponse
				{
					Success = false,
					Message = "An error occurred during OTP validation.",
					Error = ex.Message
				};
			}
		}
		public async Task<LoginResponse> LoginAgentAsync(LoginRequest data)
		{
			try
			{
				// Hash the entered password
				string enteredPasswordHash = ComputeSha256Hash(data.Password);

				// Fetch agent by email
				var agent = await _context.SupportAgents
					.FirstOrDefaultAsync(a => a.Email == data.Email);

				if (agent == null)
				{
					return new LoginResponse { Success = false, Message = "Agent not found. Please register." };
				}

				// Check if the password is correct
				if (agent.Password != enteredPasswordHash)
				{
					return new LoginResponse { Success = false, Message = "Invalid email or password." };
				}

				// Check if email is verified
				var verification = await _context.SupportEmailVerifications
					.FirstOrDefaultAsync(v => v.Email == data.Email && v.IsVerified==true);

				if (verification == null)
				{
					return new LoginResponse { Success = false, Message = "Email not verified. Please complete verification." };
				}

				// Update agent status to 'online'
				agent.Status = "online";
				agent.UpdatedAt = DateTime.Now;
				_context.SupportAgents.Update(agent);
				await _context.SaveChangesAsync();

			// Fetch role name based on agentId
				//var role = await _context.SupportRoleAssignAgents.FirstOrDefaultAsync(ra => ra.AgentID == agent.AgentId);


				//var roleName = await _context.SupportRoles.FirstOrDefaultAsync(a => a.RoleID == role.RoleID);

				// Fetch role name based on agentId
				var roleAssign = await _context.SupportRoleAssignAgents
					.FirstOrDefaultAsync(ra => ra.AgentID == agent.AgentId);
				var role = await _context.SupportRoles
					.FirstOrDefaultAsync(a => a.RoleID == roleAssign.RoleID);
				var roleName = role?.RoleName ?? "User";

				// Generate JWT Token
				var token = GenerateJwtToken(agent, roleName);

				return new LoginResponse
				{
					Success = true,
					Message = "Login successful.",
					AgentId = agent.AgentId,
					TenantId = agent.TenantId,
					Status = "online",
					RoleName = roleName,
					Token = token // Include the token in the response
				};
			}
			catch (Exception ex)
			{
				return new LoginResponse { Success = false, Message = "An error occurred during login.", Error = ex.Message };
			}
		}
		public async Task<(bool success, string message)> UpdateSocketIdAsync(string agentId, string socketId)
		{
			var agent = await _context.SupportAgents.FirstOrDefaultAsync(a => a.AgentId == agentId);

			if (agent == null)
			{
				return (false, "Agent not found.");
			}

			agent.SocketId = socketId;
			agent.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return (true, "Socket ID updated successfully.");
		}
		public async Task<(bool success, string message, string customerId)> RegisterCustomerAsync(Models.CustomerRegistrationRequest request)
		{
			// Generate a unique customer ID
			string customerId = "CUST-" + new Random().Next(10000, 99999);

			try
			{
				var customer = new SupportCustomers
				{
					CustomerId = customerId,
					TenantId = request.TenantId,
					SocketId = request.SocketId,
					CreatedAt = DateTime.UtcNow
				};

				_context.SupportCustomers.Add(customer);
				await _context.SaveChangesAsync();

				return (true, "Customer registered successfully.", customerId);
			}
			catch (Exception ex)
			{
				return (false, $"An error occurred during customer registration: {ex.Message}", null);
			}
		}
		public async Task<(bool success, string message)> UpdateCustomerSocketIdAsync(UpdateCustomerSocketRequest request)
		{
			try
			{
				var customer = await _context.SupportCustomers
											 .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

				if (customer == null)
				{
					return (false, "Customer not found.");
				}

				customer.SocketId = request.SocketId;
				customer.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				return (true, "Customer socket ID updated successfully.");
			}
			catch (Exception ex)
			{
				return (false, $"An error occurred while updating the socket ID: {ex.Message}");
			}
		}
		public async Task<(bool success, string message)> RaiseTicketAsync(RaiseTicketRequest request)
		{
			try
			{
				// Create a new ticket entity
				var ticket = new SupportTickets
				{
					TenantId = request.TenantId,
					CustomerId = request.CustomerId,
					CustomerName = request.CustomerName,
					Email = request.Email,
					Complaint = request.Complaint,
					Comments = request.Comments,
					Status = request.Status ?? "open",
					AssignedTo = request.AssignedTo ?? "Unassigned",
					Destination = request.Destination ?? "online",
					CreatedAt = DateTime.UtcNow
				};

				// Add and save the ticket to the database
				_context.SupportTickets.Add(ticket);
				await _context.SaveChangesAsync();

				return (true, "Ticket raised successfully.");
			}
			catch (Exception ex)
			{
				return (false, $"An error occurred while raising the ticket: {ex.Message}");
			}
		}
		//public async Task<(bool Success, string Message, List<TicketResponse> Tickets)> GetAllTicketsAsync(TicketRequest request)
		//{
		//	try
		//	{
		//		if (request.TenantId <= 0)
		//		{
		//			return (false, "Invalid TenantId.", null);
		//		}
		//		// Calculate offset and limit
		//		int offset = request.Start - 1;
		//		int limit = request.End - request.Start + 1;

		//		// Query with optional filters
		//		//var query = _context.SupportTickets.AsQueryable();
		//		var query = _context.SupportTickets.Where(t => t.TenantId == request.TenantId);

		//		if (!string.IsNullOrEmpty(request.AssignedTo))
		//			query = query.Where(t => t.AssignedTo == request.AssignedTo);

		//		if (!string.IsNullOrEmpty(request.Status))
		//			query = query.Where(t => t.Status == request.Status);

		//		if (!string.IsNullOrEmpty(request.Destination))
		//			query = query.Where(t => t.Destination == request.Destination);

		//		// Apply pagination
		//		var tickets = await query
		//			.OrderByDescending(t => t.CreatedAt)
		//			.Skip(offset)
		//			.Take(limit)
		//			.Select(t => new TicketResponse
		//			{
		//				TicketId = t.TicketId,
		//				TenantId = (int)t.TenantId,
		//				CustomerId = t.CustomerId,
		//				CustomerName = t.CustomerName,
		//				Comments = t.Comments,
		//				Complaint = t.Complaint,
		//				Status = t.Status,
		//				AssignedTo = t.AssignedTo,
		//				Destination = t.Destination,
		//				CreatedAt = (DateTime)t.CreatedAt
		//			})
		//			.ToListAsync();

		//		if (tickets.Any())
		//			return (true, string.Empty, tickets);
		//		else
		//			return (false, "No tickets found within the specified range and filters.", new List<TicketResponse>());
		//	}
		//	catch (Exception ex)
		//	{
		//		return (false, $"An error occurred: {ex.Message}", null);
		//	}
		//}
		public async Task<(bool Success, string Message, List<TicketResponse> Tickets, int TotalCount)> GetAllTicketsAsync(TicketRequest request)
		{
			try
			{
				if (request.TenantId <= 0)
				{
					return (false, "Invalid TenantId.", null, 0);
				}

				// Calculate offset and limit
				int offset = request.Start - 1;
				int limit = request.End - request.Start + 1;

				// Query with optional filters
				var query = _context.SupportTickets.Where(t => t.TenantId == request.TenantId);

				if (!string.IsNullOrEmpty(request.AssignedTo))
					query = query.Where(t => t.AssignedTo == request.AssignedTo);

				if (!string.IsNullOrEmpty(request.Status))
					query = query.Where(t => t.Status == request.Status);

				if (!string.IsNullOrEmpty(request.Destination))
					query = query.Where(t => t.Destination == request.Destination);

				// Get total count before pagination
				int totalCount = await query.CountAsync();

				// Apply pagination
				var tickets = await query
					.OrderByDescending(t => t.CreatedAt)
					.Skip(offset)
					.Take(limit)
					.Select(t => new TicketResponse
					{
						TicketId = t.TicketId,
						TenantId = (int)t.TenantId,
						CustomerId = t.CustomerId,
						CustomerName = t.CustomerName,
						Comments = t.Comments,
						Complaint = t.Complaint,
						Status = t.Status,
						AssignedTo = t.AssignedTo,
						Destination = t.Destination,
						CreatedAt = (DateTime)t.CreatedAt
					})
					.ToListAsync();

				if (tickets.Any())
					return (true, string.Empty, tickets, totalCount);
				else
					return (false, "No tickets found within the specified range and filters.", new List<TicketResponse>(), totalCount);
			}
			catch (Exception ex)
			{
				return (false, $"An error occurred: {ex.Message}", null, 0);
			}
		}

		public async Task<(bool Success, string Message, List<TicketResponse> Tickets)> GetTicketsByCustomerIdAsync(TicketRequestCustomer request)
		{
			try
			{
				// Retrieve tickets by customerId
				var tickets = await _context.SupportTickets
					.Where(t => t.CustomerId == request.CustomerId)
					.OrderByDescending(t => t.CreatedAt)
					.Select(t => new TicketResponse
					{
						TicketId = t.TicketId,
						TenantId = (int)t.TenantId,
						CustomerId = t.CustomerId,
						CustomerName = t.CustomerName,
						Comments = t.Comments,
						Complaint = t.Complaint,
						Status = t.Status,
						AssignedTo = t.AssignedTo,
						Destination = t.Destination,
						CreatedAt = (DateTime)t.CreatedAt
					})
					.ToListAsync();

				if (tickets.Any())
					return (true, string.Empty, tickets);
				else
					return (false, "No tickets found for the specified customer.", new List<TicketResponse>());
			}
			catch (Exception ex)
			{
				return (false, $"An error occurred: {ex.Message}", null);
			}
		}
		public async Task<UpdateTicketResponse> UpdateTicketStatusAsync(UpdateTicketRequest request)
		{
			try
			{
				// Check if the ticket exists
				var ticket = await _context.SupportTickets
					.Where(t => t.TicketId == request.TicketId)
					.FirstOrDefaultAsync();

				if (ticket == null)
				{
					return new UpdateTicketResponse
					{
						Status = "error",
						Message = "Ticket not found with the provided ID."
					};
				}

				// Update the ticket status
				ticket.Status = request.NewStatus;
				await _context.SaveChangesAsync();

				return new UpdateTicketResponse
				{
					Status = "success",
					Message = "Ticket status updated successfully.",
					TicketId = request.TicketId,
					NewStatus = request.NewStatus
				};
			}
			catch (Exception ex)
			{
				return new UpdateTicketResponse
				{
					Status = "error",
					Message = "An error occurred while updating ticket status.",
					Error = ex.Message
				};
			}
		}
		public async Task<ChatHistoryResponse> GetChatHistoryAsync(ChatHistoryRequest request)
		{
			try
			{
				// Query to get chat history between the agent and customer
				var chatHistory = await _context.SupportChats
				.Where(c => c.AgentId == request.AgentId && c.CustomerId == request.CustomerId)
				.OrderBy(c => c.CreatedAt)
				.Join(
					_context.CustomerDetailInfo,
					chat => chat.CustomerId,
					customer => customer.CustomerId,
						(chat, customer) => new ChatMessage
					{
				ChatId = chat.ChatId,
				TenantId = chat.TenantId,
				AgentId = chat.AgentId,
				CustomerId = chat.CustomerId,
				Message = chat.Message,
				Sender = chat.Sender,
				FileUrl = chat.FileUrl,
				FileName = chat.FileName,
				CustomerName = customer.Name,
				CreatedAt = (DateTime)chat.CreatedAt
			})
		.ToListAsync(); ;

				// Return response
				if (chatHistory.Any())
				{
					return new ChatHistoryResponse
					{
						Status = "success",
						Message = "Chat history retrieved successfully.",
						Data = chatHistory
					};
				}
				else
				{
					return new ChatHistoryResponse
					{
						Status = "success",
						Message = "No chat history found for the given agent and customer.",
						Data = new List<ChatMessage>()
					};
				}
			}
			catch (Exception ex)
			{
				return new ChatHistoryResponse
				{
					Status = "error",
					Message = "An error occurred while retrieving chat history.",
					Error = ex.Message
				};
			}
		}
		public async Task<AgentCustomerResponse> GetAgentCustomerListAsync(AgentCustomerRequest request)
		{
			try
			{
				// Query to get distinct customers for the given agent
				//var customers = await _context.SupportChats
				//	.Where(c => c.AgentId == request.AgentId).Join(
				//	_context.CustomerDetailInfo,
				//	supportChat => supportChat.CustomerId,
				//	customerInfo => customerInfo.CustomerId,
				// (supportChat, customerInfo) => new Customer
				// {
				//	 CustomerId = supportChat.CustomerId,
				//	 CustomerName = customerInfo.Name
				// }).Distinct().OrderBy(c => c.CustomerId).ToListAsync();


				var customers = await _context.SupportChats
	.Join(
		_context.CustomerDetailInfo,
		supportChat => supportChat.CustomerId,
		customerInfo => customerInfo.CustomerId,
		(supportChat, customerInfo) => new { supportChat, customerInfo }
	)
	.Join(
		_context.SupportAgents,
		combined => combined.supportChat.AgentId,
		agent => agent.AgentId,
		(combined, agent) => new
		{
			combined.supportChat,
			combined.customerInfo,
			agent.Name
		}
	)
	.Where(record =>
		record.supportChat.AgentId == request.AgentId ||
		record.Name == request.AgentId)
	.Select(record => new Customer
	{
		CustomerId = record.supportChat.CustomerId,
		CustomerName = record.customerInfo.Name
	})
	.Distinct()
	.OrderBy(c => c.CustomerId)
	.ToListAsync();


				// Return response
				if (customers.Any())
				{
					return new AgentCustomerResponse
					{
						Status = "success",
						Message = "Customer list retrieved successfully.",
						Customers = customers
					};
				}
				else
				{
					return new AgentCustomerResponse
					{
						Status = "success",
						Message = "No customers found for the given agent.",
						Customers = new List<Customer>()
					};
				}
			}
			catch (Exception ex)
			{
				return new AgentCustomerResponse
				{
					Status = "error",
					Message = "An error occurred while retrieving the customer list.",
					Error = ex.Message
				};
			}
		}
		public async Task<AgentResponse> GetAgentsAsync(AgentRequest request)
		{
			try
			{
				// Query to get agents for the given tenantId
				var agents = await _context.SupportAgents
					.Where(a => a.TenantId == request.TenantId)
					.Select(a => new Agent
					{
						AgentId = a.AgentId,
						AgentName = a.Name,
						AgentEmail = a.Email
					})
					.ToListAsync();

				// Return response
				if (agents.Any())
				{
					return new AgentResponse
					{
						Success = true,
						Agents = agents
					};
				}
				else
				{
					return new AgentResponse
					{
						Success = true,
						Agents = new List<Agent>()
					};
				}
			}
			catch (Exception ex)
			{
				return new AgentResponse
				{
					Success = false,
					Message = "An error occurred while retrieving agents.",
					Error = ex.Message
				};
			}
		}
		public async Task<AgentResponseById> GetAgentByIdAsync(string agentId)
		{
			try
			{
				// Query to get the agent by agentId
				var agent = await _context.SupportAgents
					.Where(a => a.AgentId == agentId)
					.Select(a => new Agent
					{
						AgentId = a.AgentId,
						AgentName = a.Name,
						AgentEmail = a.Email
					})
					.FirstOrDefaultAsync();

				if (agent != null)
				{
					return new AgentResponseById
					{
						Success = true,
						Agent = agent
					};
				}
				else
				{
					return new AgentResponseById
					{
						Success = false,
						Message = "Agent not found with the provided ID."
					};
				}
			}
			catch (Exception ex)
			{
				return new AgentResponseById
				{
					Success = false,
					Message = "An error occurred while retrieving agent details.",
					Error = ex.Message
				};
			}
		}
		public async Task<AssignTicketResponse> AssignTicketAsync(AssignTicketRequest request)
		{
			try
			{
				// Validate inputs
				if (request.TicketId <= 0 || string.IsNullOrWhiteSpace(request.AgentId))
				{
					return new AssignTicketResponse
					{
						Success = false,
						Message = "Invalid ticketId or agentId provided."
					};
				}

				// Find the ticket by ID
				var ticket = await _context.SupportTickets.FindAsync(request.TicketId);
				if (ticket == null)
				{
					return new AssignTicketResponse
					{
						Success = false,
						Message = "Ticket not found."
					};
				}

				// Update the ticket assignment
				ticket.AssignedTo = request.AgentId;
				_context.SupportTickets.Update(ticket);
				await _context.SaveChangesAsync();

				return new AssignTicketResponse
				{
					Success = true,
					Message = "Ticket successfully assigned to agent.",
					TicketId = request.TicketId,
					AssignedTo = request.AgentId
				};
			}
			catch (Exception ex)
			{
				return new AssignTicketResponse
				{
					Success = false,
					Message = "An error occurred while assigning the ticket.",
					Error = ex.Message
				};
			}
		}
		public async Task<TicketResponseByAgentid> GetTicketsByAssignedToAsync(string agentId)
		{
			try
			{
				// Fetch tickets; filter by agentId if provided
				var query = _context.SupportTickets.AsQueryable();

				
					query = query.Where(ticket => ticket.AssignedTo == agentId);
				

				var ticketsLists = await query
					.OrderByDescending(ticket => ticket.CreatedAt)
					.ToListAsync();

				return new TicketResponseByAgentid
				{
					Status = "success",
					Message = ticketsLists.Count != 0
						? "Tickets retrieved successfully."
						: "No tickets found for the specified agent {agentId}",
					Tickets = ticketsLists
				};
			}
			catch (Exception ex)
			{
				return new TicketResponseByAgentid
				{
					Status = "error",
					Message = "An error occurred while retrieving tickets.",
					Error = ex.Message
				};
			}
		}
		public async Task<CustomerResponse> PostCustomerInfo(CustomerRequest data)
		{
			try
			{
				
				// Insert chat message into the database
				var customerDetail = new CustomerDetailInfo
				{
					CustomerId = data.CustomerId,
					Name = data.Name,
					Email = data.Email,
					Issue = data.Issue,
					Description = data.Description,
					IP = data.IP,
					DeviceType = data.DeviceType,
					Browser = data.Browser,
					City = data.City,
					CountryName = data.CountryName,
					CountryCode = data.CountryCode,
					Region = data.Region,
					DateCreated = DateTime.UtcNow, // Set creation date
					DateUpdated = null // This is null when a new record is created
				};

				_context.CustomerDetailInfo.Add(customerDetail);
				await _context.SaveChangesAsync();

				return new CustomerResponse { Success = true, Message = "Chat message and file uploaded successfully." };
			}
			catch (Exception ex)
			{
				return new CustomerResponse { Success = false, Message = "An error occurred while posting the chat message." };
			}
		}

		public async Task<CustomerResponse> GetCustomerInfo(string customerId)
		{
			try
			{
				// Fetch customer detail from the database by CustomerId
				var customerDetail = await _context.CustomerDetailInfo
					.FirstOrDefaultAsync(c => c.CustomerId == customerId);


				// Check if customer detail exists
				if (customerDetail == null)
				{
					return new CustomerResponse
					{
						Success = false,
						Message = "Customer not found."
					};
				}
				var customerRequest = new CustomerRequest
				{
					CustomerId = customerDetail.CustomerId,
					Name = customerDetail.Name,
					Email = customerDetail.Email,
					Issue = customerDetail.Issue,
					Description = customerDetail.Description,
					IP = customerDetail.IP,
					DeviceType = customerDetail.DeviceType,
					Browser = customerDetail.Browser,
					City = customerDetail.City,
					CountryName = customerDetail.CountryName,
					CountryCode = customerDetail.CountryCode,
					Region = customerDetail.Region
				};

				// Return the customer data as part of the response (you can modify this to include the customer data)
				return new CustomerResponse
				{
					Success = true,
					Message = "Customer details fetched successfully.",
					CustomerDetails = customerRequest

					// You can map customerDetail to a response model here if necessary
				};
			}
			catch (Exception ex)
			{
				return new CustomerResponse
				{
					Success = false,
					Message = "An error occurred while retrieving the customer details.",
					
				};
			}
		}

		public async Task<CustomerResponse> AddCustomerFeedbackAsync(CustomerFeedbackDto feedbackDto)
		{
			try
			{
				// Validate feedback data
				if (feedbackDto == null || feedbackDto.Rating < 1 || feedbackDto.Rating > 5)
				{
					return new CustomerResponse
					{
						Success = false,
						Message = "Invalid feedback data."
					};
				}

				// Set creation timestamp
				var feedback = new SupportCustomerRating
				{
					CustomerId = feedbackDto.CustomerId,
					AgentId = feedbackDto.AgentId,
					Rating = feedbackDto.Rating,
					CreateDate = DateTime.UtcNow  // Set creation timestamp
				};


				// Add feedback to the database
				_context.SupportCustomerRatings.Add(feedback);
				await _context.SaveChangesAsync();

				return new CustomerResponse
				{
					Success = true,
					Message = "Feedback submitted successfully!"
				};
			}
			catch (Exception ex)
			{
				// Return error response
				return new CustomerResponse
				{
					Success = false,
					Message = "An error occurred while processing the feedback.",
				
				};
			}
		}
		private string GenerateJwtToken(SupportAgents agent, string roleName)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			// Define the claims
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, agent.AgentId),
				new Claim(JwtRegisteredClaimNames.Email, agent.Email),
				new Claim(ClaimTypes.Role, roleName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			// Create the token
			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		//password 
		public async Task<bool> IsEmailValidAsync(string email)
		{
			return await _context.SupportAgents.AnyAsync(a => a.Email == email);
		}

		public async Task AddForgotPwdVerificationAsync(string email, string verificationCode)
		{
			var verification = new SupportAgentForgotPwdVerification
			{
				Email = email,
				VerificationCode = verificationCode,
				IsVerified = false,
				CreatedAt = DateTime.UtcNow,
				ExpiredAt = DateTime.UtcNow.AddMinutes(15) // OTP valid for 15 minutes
			};

			_context.supportAgentForgotPwdVerifications.Add(verification);
			await _context.SaveChangesAsync();
		}

		public async Task<SupportAgentForgotPwdVerification> GetForgotPwdVerificationAsync(string email, string verificationCode)
		{
			return await _context.supportAgentForgotPwdVerifications
				.FirstOrDefaultAsync(v => v.Email == email && v.VerificationCode == verificationCode && !v.IsVerified && v.ExpiredAt > DateTime.UtcNow);
		}

		public async Task MarkOtpAsVerifiedAsync(int verificationId)
		{
			var verification = await _context.supportAgentForgotPwdVerifications.FindAsync(verificationId);
			if (verification != null)
			{
				verification.IsVerified = true;
				_context.supportAgentForgotPwdVerifications.Update(verification);
				await _context.SaveChangesAsync();
			}
		}

		public async Task UpdateAgentPasswordAsync(string email, string newHashedPassword)
		{
			var agent = await _context.SupportAgents.FirstOrDefaultAsync(a => a.Email == email);
			if (agent != null)
			{
				agent.Password = newHashedPassword;
				_context.SupportAgents.Update(agent);
				await _context.SaveChangesAsync();
			}
		}

		public async Task LogPasswordChangeAsync(string agentId, string changedBy, string remarks)
		{
			var log = new PasswordChangeLog
			{
				AgentId = agentId,
				ChangedAt = DateTime.UtcNow,
				ChangedBy = changedBy,
				Remarks = remarks
			};

			_context.PasswordChangeLogs.Add(log);
			await _context.SaveChangesAsync();
		}
		private string ComputeSha256Hash(string rawData)
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}
				return builder.ToString();
			}
		}
	}
}
