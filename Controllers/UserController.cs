using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Varadhi.Models;
using Varadhi.Services;

namespace Varadhi.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserController(IAgentService agentService, IEmailService emailService, IConfiguration configuration)
        {
            _agentService = agentService;
            _emailService = emailService;
            _configuration = configuration;
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
                    From = "support@marketcentral.in",
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
