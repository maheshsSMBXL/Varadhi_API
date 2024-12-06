using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Varadhi.Data;
using Varadhi.Models;
using Varadhi.Services;

namespace Varadhi.Controllers
{
    public class UserController : ControllerBase
    {
		private readonly ApplicationDbContext _context;
		private readonly IAgentService _agentService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
		private readonly IAgentCustomerService _agentCustomerService;
        private readonly IChatService _chatService;
		public UserController(ApplicationDbContext context,IAgentService agentService, IEmailService emailService, IConfiguration configuration, IAgentCustomerService agentCustomerService,IChatService chatService) 
        {
			_context = context;
			_agentService = agentService;
            _emailService = emailService;
            _configuration = configuration;
			_agentCustomerService = agentCustomerService;
            _chatService = chatService;
		}
		

			
		
		[HttpPost("registerAgent")]
        public async Task<IActionResult> RegisterAgent([FromBody] RegisterAgentRequest request)
        {
            try
            {
                // Generate custom agentId
                var agentId = "AGENT-" + new Random().Next(10000, 99999).ToString();

                // Hash the password using SHA-256
                var hashedPassword = HashPassword(request.Password);

                // Insert agent data into the database
                await _agentService.AddAgentAsync(agentId, request.Name, request.Email, hashedPassword, request.TenantId);

                // Insert Role Assignment for Agent
                await _agentService.AssignRoleToAgentAsync(agentId, request.RoleId);

                // Generate a 5-digit email verification code
                var verificationCode = new Random().Next(10000, 99999).ToString();

                // Insert verification code into EMAIL_VERIFICATION table
                await _agentService.InsertEmailVerificationAsync(request.Email, verificationCode, agentId);

                // Prepare email content
                var emailData = new EmailData
                {
                    From = "chatsupportops@personalizedhealthrx.com",
                    To = request.Email,
                    Subject = "Please Verify Your Email",
                    Body = $"Thank you for registering. Please use the following OTP to verify your email: {verificationCode}",
                    Type = "html"
                };

                // Send OTP email
                await _emailService.SendEmailAsync(emailData);

                // Return success response
                return Ok(new
                {
                    success = true,
                    message = "Agent registered successfully. OTP has been sent to your email.",
                    agentId = agentId,
                    role = request.RoleId == 1 ? "Agent" : "Admin"
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

		[HttpPost("assignCustomerToAgent")]
		public async Task<IActionResult> AssignCustomerToAgent([FromBody] AssignmentRequest request)
		{
			var response = await _agentCustomerService.AssignCustomerToAgentAsync(request);

			if (response.Status == "error")
			{
				return BadRequest(response);
			}

			return Ok(response);
		}
		[HttpPost("postChatMessage")]
		public async Task<IActionResult> PostChatMessage([FromBody] ChatRequest data)
		{
			var response = await _chatService.PostChatMessageAsync(data);

			if (response.Success)
				return Ok(response);
			else
				return BadRequest(response);
		}
		[HttpPost("validateOTP")]
		public async Task<IActionResult> ValidateOTP([FromBody] OTPRequest data)
		{
			var response = await _agentCustomerService.ValidateOTPAsync(data);

			if (response.Success)
				return Ok(response);
			else
				return BadRequest(response);
		}

		[HttpPost("loginAgent")]
		public async Task<IActionResult> LoginAgent([FromBody] LoginRequest data)
		{
			var response = await _agentCustomerService.LoginAgentAsync(data);

			if (response.Success)
				return Ok(response);
			else
				return BadRequest(response);
		}

		[HttpPost("updateAgentSocketId")]
		public async Task<IActionResult> UpdateAgentSocketId([FromBody] UpdateSocketIdRequest request)
		{
			if (string.IsNullOrEmpty(request.AgentId) || string.IsNullOrEmpty(request.SocketId))
			{
				return BadRequest(new { status = "error", message = "AgentId and SocketId are required." });
			}

			var (success, message) = await _agentCustomerService.UpdateSocketIdAsync(request.AgentId, request.SocketId);

			if (success)
			{
				return Ok(new { status = "success", message });
			}

			return NotFound(new { status = "error", message });
		}
		[HttpPost("registerCustomer")]
		public async Task<IActionResult> RegisterCustomer([FromBody] Models.CustomerRegistrationRequest request)
		{
			if (request.TenantId <= 0 || string.IsNullOrEmpty(request.SocketId))
			{
				return BadRequest(new { success = false, message = "TenantId and SocketId are required." });
			}

			var (success, message, customerId) = await _agentCustomerService.RegisterCustomerAsync(request);

			if (success)
			{
				return Ok(new { success = true, message, customerId });
			}

			return StatusCode(500, new { success = false, message });
		}
		[HttpPost("updateCustomerSocketId")]
		public async Task<IActionResult> UpdateCustomerSocketId([FromBody] UpdateCustomerSocketRequest request)
		{
			if (string.IsNullOrEmpty(request.CustomerId) || string.IsNullOrEmpty(request.SocketId))
			{
				return BadRequest(new { status = "error", message = "customerId and socketId are required." });
			}

			var (success, message) = await _agentCustomerService.UpdateCustomerSocketIdAsync(request);

			if (success)
			{
				return Ok(new { status = "success", message });
			}

			return StatusCode(500, new { status = "error", message });
		}
		[HttpPost("raiseTicket")]
		public async Task<IActionResult> RaiseTicket([FromBody] RaiseTicketRequest request)
		{
			// Validate required fields
			if (request.TenantId <= 0 || string.IsNullOrEmpty(request.CustomerId) || string.IsNullOrEmpty(request.Complaint))
			{
				return BadRequest(new { status = "error", message = "tenantId, customerId, and complaint are required fields." });
			}

			var (success, message) = await _agentCustomerService.RaiseTicketAsync(request);

			if (success)
			{
				return Ok(new { status = "success", message });
			}

			return StatusCode(500, new { status = "error", message });
		}
		[HttpPost("getAllTickets")]
		public async Task<IActionResult> GetAllTickets([FromBody] TicketRequest request)
		{
			// Validate pagination parameters
			if (request.Start <= 0 || request.End <= 0 || request.End < request.Start)
			{
				return BadRequest(new { status = "error", message = "Invalid pagination parameters. Ensure 'start' and 'end' are positive and 'end' is greater than 'start'." });
			}

			var result = await _agentCustomerService.GetAllTicketsAsync(request);

			if (result.Success)
				return Ok(new { status = "success", message = "Tickets retrieved successfully.", tickets = result.Tickets,totaltickets = result.TotalCount });
			else
				return NotFound(new { status = "error", message = result.Message });
		}
		[HttpPost("getTicketsByCustomerId")]
		public async Task<IActionResult> GetTicketsByCustomerId([FromBody] TicketRequestCustomer request)
		{
			if (string.IsNullOrEmpty(request.CustomerId))
			{
				return BadRequest(new { status = "error", message = "CustomerId is required." });
			}

			var result = await _agentCustomerService.GetTicketsByCustomerIdAsync(request);

			if (result.Success)
				return Ok(new { status = "success", message = "Tickets retrieved successfully.", tickets = result.Tickets });
			else
				return NotFound(new { status = "error", message = result.Message });
		}
		[HttpPost("updateTicketStatus")]
		public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketRequest request)
		{
			if (request.TicketId <= 0 || string.IsNullOrEmpty(request.NewStatus))
			{
				return BadRequest(new UpdateTicketResponse
				{
					Status = "error",
					Message = "TicketId and NewStatus are required."
				});
			}

			var result = await _agentCustomerService.UpdateTicketStatusAsync(request);

			if (result.Status == "success")
			{
				return Ok(result);
			}
			else
			{
				return NotFound(result);
			}
		}
		[HttpPost("getChatHistory")]
		public async Task<IActionResult> GetChatHistory([FromBody] ChatHistoryRequest request)
		{
			if (string.IsNullOrEmpty(request.AgentId) || string.IsNullOrEmpty(request.CustomerId))
			{
				return BadRequest(new ChatHistoryResponse
				{
					Status = "error",
					Message = "AgentId and CustomerId are required."
				});
			}

			var result = await _agentCustomerService.GetChatHistoryAsync(request);

			if (result.Status == "success")
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result);
			}
		}
		[HttpPost("getAgentCustomerList")]
		public async Task<IActionResult> GetAgentCustomerList([FromBody] AgentCustomerRequest request)
		{
			if (string.IsNullOrEmpty(request.AgentId))
			{
				return BadRequest(new AgentCustomerResponse
				{
					Status = "error",
					Message = "AgentId is required."
				});
			}

			var result = await _agentCustomerService.GetAgentCustomerListAsync(request);

			if (result.Status == "success")
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result);
			}
		}
     
		[HttpPost("getAgents")]
		public async Task<IActionResult> GetAgents([FromBody] AgentRequest request)
		{
			if (request.TenantId=="")
			{
				return BadRequest(new AgentResponse
				{
					Success = false,
					Message = "TenantId is required."
				});
			}

			var result = await _agentCustomerService.GetAgentsAsync(request);

			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result);
			}
		}
		[HttpGet("getAgentById/{agentId}")]
		public async Task<IActionResult> GetAgentById(string agentId)
		{
			// Check if agentId is provided
			if (string.IsNullOrEmpty(agentId))
			{
				return BadRequest(new AgentResponse
				{
					Success = false,
					Message = "AgentId is required."
				});
			}

			// Fetch agent details by agentId using the service
			var result = await _agentCustomerService.GetAgentByIdAsync(agentId);

			// Return appropriate response
			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return NotFound(result);
			}
		}

		[HttpPost("assignTicket")]
		public async Task<IActionResult> AssignTicket([FromBody] AssignTicketRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new AssignTicketResponse
				{
					Success = false,
					Message = "Invalid input data."
				});
			}

			var result = await _agentCustomerService.AssignTicketAsync(request);

			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result);
			}
		}
		[HttpGet("getTicketsByAssignedTo/{agentId}")]
		public async Task<IActionResult> GetTicketsByAssignedTo(string agentId)
		{
			var result = await _agentCustomerService.GetTicketsByAssignedToAsync(agentId);

			if (result.Status == "success")
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result);
			}
		}
		[HttpPost("postCustomerInfo")]
		public async Task<IActionResult> PostcustomerInfo([FromBody] CustomerRequest data)
		{
			var response = await _agentCustomerService.PostCustomerInfo(data);

			if (response.Success)
				return Ok(response);
			else
				return BadRequest(response);
		}
		[HttpGet("getCustomerInfo/{customerId}")]
		public async Task<IActionResult> GetCustomerInfo(string customerId)
		{
			var response = await _agentCustomerService.GetCustomerInfo(customerId);

			if (response.Success)
				return Ok(response); // Returns HTTP 200 with response
			else
				return NotFound(response); // Returns HTTP 404 if customer is not found
		}
		[HttpPost("PostCustomerFeedback")]
		public async Task<IActionResult> PostCustomerFeedback([FromBody] CustomerFeedbackDto feedbackDto)
		{

			var response = await _agentCustomerService.AddCustomerFeedbackAsync(feedbackDto);
			if (response.Success)
				return Ok(response); // Returns HTTP 200 with response
			else
				return NotFound(response); // Returns HTTP 404 if customer is not found
		}

		[HttpGet("getagentStats/{agentId}")]
		public async Task<ActionResult<AgentStatsDto>> GetAgentStats(string agentId, DateTime startDate, DateTime endDate)
		{
			// Ensure the dates are date-only (time component set to 00:00:00)
			DateTime start = startDate.Date;
			DateTime end = endDate.Date.AddDays(1); // Include the end date in the range

			var totalCustomersQuery =
				from a in _context.SupportAgentCustomerAssignment
				join r in _context.SupportCustomerRatings on new { a.AgentId, a.CustomerId } equals new { r.AgentId, r.CustomerId } into ratings
				from r in ratings.DefaultIfEmpty()
				where a.AgentId == agentId && a.AssignedAt >= start && a.AssignedAt < end
				group new { a, r } by a.CustomerId into g
				select new
				{
					CustomerId = g.Key,
					Rating = g.Max(x => x.r != null ? x.r.Rating : 0)
				};

			var totalCustomers = await totalCustomersQuery.CountAsync();
			var totalPossibleRatingPoints = totalCustomers * 5;
			var totalRatingPointsReceived = await totalCustomersQuery.SumAsync(x => x.Rating);
			var customerSatisfactionRatePercentage = totalPossibleRatingPoints > 0
				? (totalRatingPointsReceived * 100.0) / totalPossibleRatingPoints
				: 0.0;

			var ticketStatsQuery =
				from t in _context.SupportTickets
				where t.AssignedTo == agentId
				group t by t.AssignedTo into g
				select new
				{
					AgentId = g.Key,
					OpenTickets = g.Count(t => t.Status == "Open"),
					ClosedTickets = g.Count(t => t.Status == "Closed")
				};

			var ticketStats = await ticketStatsQuery.FirstOrDefaultAsync();

			var result = new AgentStatsDto
			{
				AgentId = agentId,
				TotalConnectedCustomers = totalCustomers,
				TotalPossibleRatingPoints = totalPossibleRatingPoints,
				TotalRatingPointsReceived = totalRatingPointsReceived,
				CustomerSatisfactionRatePercentage = customerSatisfactionRatePercentage,
				OpenTickets = ticketStats?.OpenTickets ?? 0,
				ClosedTickets = ticketStats?.ClosedTickets ?? 0
			};

			return Ok(result);
		}


		private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
}
