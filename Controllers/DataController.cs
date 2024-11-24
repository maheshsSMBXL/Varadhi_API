﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Varadhi.Data;
using Varadhi.Models;
using Varadhi.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Agri_Smart.Helpers;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Varadhi.Controllers
{
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private const string EncryptionKey = "12345678901234567890123456789012"; // Ensure this is 32 characters long
        private const string FixedIV = "1234567890123456"; // Ensure this is 16 characters long

        public DataController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("GetAgentsByTenant/{tenantId}")]
        public async Task<IActionResult> GetAgentsByTenant(int tenantId)
        {
            try
            {
                // Validate if the tenant exists
                var tenantExists = await _context.SupportTenants.AnyAsync(t => t.TenantId == tenantId);
                if (!tenantExists)
                {
                    return Ok(new ResponseDto<List<SupportAgentDto>>
                    {
                        Success = false,
                        Message = "Invalid Tenant ID. Tenant does not exist.",
                        Data = null
                    });
                }

                // Retrieve agents for the given Tenant ID who are registered and validated
                var agents = await (from agent in _context.SupportAgents
                                    join roleAssign in _context.SupportRoleAssignAgents on agent.AgentId equals roleAssign.AgentID
                                    join role in _context.SupportRoles on roleAssign.RoleID equals role.RoleID
                                    where agent.TenantId == tenantId.ToString() && _context.SupportEmailVerifications.Any(ev => ev.AgentId == agent.AgentId && ev.IsVerified == true)
                                    select new SupportAgentDto
                                    {
                                        AgentId = agent.AgentId,
                                        Name = agent.Name,
                                        Email = agent.Email,
                                        Status = agent.Status,
                                        Role = role.RoleName,
                                        CreatedAt = agent.CreatedAt ?? DateTime.MinValue
                                    }).ToListAsync();

                // Prepare the response with agent details
                return Ok(new ResponseDto<List<SupportAgentDto>>
                {
                    Success = true,
                    Message = null,
                    Data = agents
                });
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, new ResponseDto<string>
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message,
                    Data = null
                });
            }
        }
        [HttpPost("updateAgentRole")]
        public async Task<IActionResult> UpdateAgentRole([FromBody] UpdateAgentRoleDto data)
        {
            if (data == null || !ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<string>
                {
                    Success = false,
                    Message = "Invalid input."
                });
            }

            try
            {
                // Validate if the tenant exists
                var tenantExists = await _context.SupportTenants.AnyAsync(t => t.TenantId == data.TenantId);
                if (!tenantExists)
                {
                    return Ok(new ResponseDto<string>
                    {
                        Success = false,
                        Message = "Invalid or missing Tenant ID."
                    });
                }

                // Validate if the agent exists for the given tenant
                var agent = await (from ra in _context.SupportRoleAssignAgents
                                   join a in _context.SupportAgents on ra.AgentID equals a.AgentId
                                   join r in _context.SupportRoles on ra.RoleID equals r.RoleID
                                   where a.AgentId == data.AgentId && a.TenantId == data.TenantId.ToString()
                                   select new { a.AgentId, r.RoleName }).FirstOrDefaultAsync();

                if (agent == null)
                {
                    return Ok(new ResponseDto<string>
                    {
                        Success = false,
                        Message = "Agent not found in the given tenant."
                    });
                }

                // Update the agent's role in the tenant
                var roleAssignment = await _context.SupportRoleAssignAgents
                    .FirstOrDefaultAsync(ra => ra.AgentID == data.AgentId);

                if (roleAssignment != null)
                {
                    roleAssignment.RoleID = data.NewRoleId;
                    await _context.SaveChangesAsync();
                }

                return Ok(new ResponseDto<UpdateAgentRoleDto>
                {
                    Success = true,
                    Message = "Agent role updated successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, new ResponseDto<string>
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message,
                    Data = null
                });
            }
        }
        [HttpGet("ListInvitationsByTenant/{tenantId}")]
        public async Task<IActionResult> ListInvitationsByTenant(int tenantId)
        {
            try
            {
                // Validate if the tenant exists
                var tenantExists = await _context.SupportTenants
                    .AnyAsync(t => t.TenantId == tenantId);

                if (!tenantExists)
                {
                    return Ok(new ResponseDto<string>
                    {
                        Success = false,
                        Message = "Invalid Tenant ID. Tenant does not exist."
                    });
                }

                // Retrieve invitations for the given Tenant ID
                var invitations = await _context.SupportInvitations
                    .Where(i => i.TenantID == tenantId.ToString())
                    .Join(_context.SupportRoles, i => i.RoleID, r => r.RoleID, (i, r) => new InvitationDto
                    {
                        InvitationId = i.InvitationID,
                        Email = i.Email,
                        Status = i.Status,
                        Role = r.RoleName,
                        CreatedAt = i.CreatedAt.ToString()
                    })
                    .ToListAsync();

                return Ok(new ResponseDto<List<InvitationDto>>
                {
                    Success = true,
                    Data = invitations
                });
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, new ResponseDto<string>
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }
        [HttpPost("ResendInvitation")]
        public async Task<IActionResult> ResendInvitation([FromBody] ResendInvitationRequest request)
        {
            try
            {
                // Validate the input
                if (request.TenantId <= 0)
                {
                    return Ok(new ResponseDto<string>
                    {
                        Success = false,
                        Message = "Invalid or missing Tenant ID."
                    });
                }

                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    return Ok(new ResponseDto<string>
                    {
                        Success = false,
                        Message = "Invalid or missing email address."
                    });
                }

                // Check if a pending invitation exists for the given email and tenant ID
                var pendingInvitation = await _context.SupportInvitations
                    .FirstOrDefaultAsync(i => i.Email == request.Email && i.TenantID == request.TenantId.ToString() && i.Status == "Pending");

                if (pendingInvitation == null)
                {
                    return Ok(new ResponseDto<string>
                    {
                        Success = false,
                        Message = "No pending invitation found for this email and tenant."
                    });
                }

                // Prepare the data to encrypt (tenantId, roleId, invitationId)
                var dataToEncrypt = new
                {
                    tenantId = pendingInvitation.TenantID,
                    roleId = pendingInvitation.RoleID,
                    invitationId = pendingInvitation.InvitationID
                };

                // Encrypt the data using AES-256
                var encryptedData = EncryptData(EncryptionKey, FixedIV, dataToEncrypt);
                var invitationLink = $"https://agent.com/?invitation={encryptedData}";

                // Prepare email content for the invitation
                var emailData = new EmailData
                {
                    From = "support@marketcentral.in",
                    To = request.Email,
                    Subject = "Invitation Resent: Join as an Agent",
                    Body = $"Hello,<br><br>You have been re-invited to join as an agent for Tenant ID: {pendingInvitation.TenantID}.<br><br>Your role will be: {(pendingInvitation.RoleID == 1 ? "Agent" : "Admin")}.<br><br>Use the following link to accept your invitation:<br><a href='{invitationLink}'>{invitationLink}</a><br><br>Thank you,<br>MarketCentral Support Team",
                    Type = "html"
                };

                // Call sendEmail function to resend the invitation email
                await _emailService.SendEmailAsync(emailData);

                // Return success response
                return Ok(new ResponseDto<string>
                {
                    Success = true,
                    Message = "Invitation resent successfully.",
                    Data = invitationLink // Optional: Remove this if you don't want to expose the link in the API response
                });
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, new ResponseDto<string>
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string EncryptData(string key, string iv, object data)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var bytes = Encoding.UTF8.GetBytes(json);

                var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return Convert.ToBase64String(encrypted);
            }
        }
        [HttpPost("DecryptInvitation")]
        public async Task<IActionResult> DecryptInvitation([FromBody] DecryptInvitationRequest request)
        {
            try
            {
                // Validate and extract the encrypted data
                if (string.IsNullOrEmpty(request.EncryptedData))
                {
                    throw new ArgumentException("Missing 'encryptedData' in request body.");
                }

                // Decrypt the data using AES-256
                var decryptedJson = DecryptData(EncryptionKey, FixedIV, request.EncryptedData);
                var decryptedData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson);

                // Extract tenantId and roleId from decrypted data
                if (!decryptedData.ContainsKey("tenantId") || !decryptedData.ContainsKey("roleId"))
                {
                    throw new ArgumentException("Decrypted data does not contain 'tenantId' or 'roleId'.");
                }

                var tenantId = decryptedData["tenantId"];
                var roleId = decryptedData["roleId"];

                // Fetch tenantKey from the database
                var tenant = await _context.SupportTenants.FirstOrDefaultAsync(t => t.TenantId == int.Parse(tenantId));
                if (tenant == null)
                {
                    throw new ArgumentException("Tenant not found.");
                }

                var tenantKey = tenant.TenantKey;

                // Fetch role details from the database (optional)
                var role = await _context.SupportRoles.FirstOrDefaultAsync(r => r.RoleID == int.Parse(roleId));
                if (role == null)
                {
                    throw new ArgumentException("Role not found.");
                }

                var roleName = role.RoleName;

                // Prepare the success response
                var response = new DecryptInvitationResponse
                {
                    Success = true,
                    TenantId = tenantId,
                    TenantKey = tenantKey,
                    RoleId = roleId,
                    RoleName = roleName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors and prepare the error response
                var response = new DecryptInvitationResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                };

                return StatusCode(500, response);
            }
        }

        private static string DecryptData(string key, string iv, string encryptedData)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        [HttpPost("InviteAgent")]
        public async Task<IActionResult> InviteAgent([FromBody] InviteAgentRequest request)
        {
            try
            {
                // Generate a secret invitation ID
                var invitationId = Guid.NewGuid().ToString();

                // Validate if tenant exists
                var tenant = await _context.SupportTenants.FirstOrDefaultAsync(t => t.TenantId == request.TenantId);
                if (tenant == null)
                {
                    return BadRequest(new InviteAgentResponse
                    {
                        Success = false,
                        Message = "Invalid Tenant ID. Tenant does not exist."
                    });
                }

                // Insert invitation details into TBL_MCSUPPORT_INVITATIONS
                var invitation = new SupportInvitations
                {
                    InvitationID = invitationId,
                    TenantID = request.TenantId.ToString(),
                    RoleID = request.RoleId,
                    Email = request.Email,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                _context.SupportInvitations.Add(invitation);
                await _context.SaveChangesAsync();

                // Prepare the data to encrypt
                var dataToEncrypt = new
                {
                    tenantId = request.TenantId,
                    roleId = request.RoleId,
                    invitationId = invitationId
                };

                // Encrypt the data using a secret key
                var encryptedData = EncryptData(EncryptionKey, FixedIV, JsonSerializer.Serialize(dataToEncrypt));

                // Generate the invitation link
                var invitationLink = $"https://agent.com/?invitation={Uri.EscapeDataString(encryptedData)}";

                // Prepare email content for the invitation
                var emailData = new EmailData
                {
                    From = "support@marketcentral.in",
                    To = request.Email,
                    Subject = "You're Invited to Join as an Agent",
                    Body = $"Hello,<br><br>You have been invited to join as an agent for Tenant ID: {request.TenantId}.<br><br>Your role will be: {(request.RoleId == 1 ? "Agent" : "Admin")}.<br><br>Use the following link to accept your invitation:<br><a href='{invitationLink}'>{invitationLink}</a><br><br>Thank you,<br>MarketCentral Support Team",
                    Type = "html"
                };

                // Call sendEmail function to send the invitation email
                await _emailService.SendEmailAsync(emailData);

                // Return success response
                var response = new InviteAgentResponse
                {
                    Success = true,
                    Message = "Agent invited successfully. Invitation email has been sent.",
                    InvitationLink = invitationLink // Optional: You can omit this if you don't want to expose the link in the API response
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors
                var response = new InviteAgentResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                };

                return StatusCode(500, response);
            }
        }
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Verify the provided verification code
                var verification = await _context.SupportPasswordChangeVerification
                    .FirstOrDefaultAsync(v => v.VerificationCode == request.VerificationCode && v.IsUsed == false);

                if (verification == null)
                {
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "Invalid or expired verification code."
                    });
                }

                // Hash the new password
                var salt = Guid.NewGuid().ToString();
                var newPasswordHash = HashPassword(request.NewPassword, salt);

                // Update the password
                var tenantPassword = await _context.SupportTenantPassword.FirstOrDefaultAsync(t => t.TenantId == verification.TenantID);
                if (tenantPassword != null)
                {
                    tenantPassword.PasswordHash = newPasswordHash;
                    tenantPassword.Salt = salt;
                }
                else
                {
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "Tenant not found."
                    });
                }

                // Mark the verification code as used
                verification.IsUsed = true;
                await _context.SaveChangesAsync();

                // Return success response
                var response = new ChangePasswordResponse
                {
                    Success = true,
                    Message = "Password updated successfully."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors
                var response = new ChangePasswordResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                };

                return StatusCode(500, response);
            }
        }

        private static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        [HttpPost("LoginTenant")]
        public async Task<IActionResult> LoginTenant([FromBody] LoginRequest request)
        {
            try
            {
                // Retrieve Tenant's password hash and salt
                var supportTenants = await _context.SupportTenants.FirstOrDefaultAsync(a => a.AdminEmail == request.Email);
                var tenantPassword = await _context.SupportTenantPassword.FirstOrDefaultAsync(p => p.TenantId == supportTenants.TenantId);

                if (tenantPassword == null)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }

                // Validate the password
                var hashedInputPassword = HashPassword(request.Password, tenantPassword.Salt);
                if (hashedInputPassword != tenantPassword.PasswordHash)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }

                // Return success response
                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    TenantId = tenantPassword.TenantId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors
                var response = new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                };

                return StatusCode(500, response);
            }
        }
        [HttpPost("VerifyTenant")]
        public async Task<IActionResult> VerifyTenant([FromBody] VerifyTenantRequest request)
        {
            try
            {
                // Check if the TenantKey and VerificationCode match
                var tenant = await _context.SupportTenants
                    .FirstOrDefaultAsync(t => t.TenantKey == request.TenantKey && t.VerificationCode == request.VerificationCode);

                if (tenant == null)
                {
                    // Handle no matching tenant
                    return BadRequest(new VerifyTenantResponse
                    {
                        Success = false,
                        Message = "Invalid verification code or TenantKey."
                    });
                }

                // Handle already verified tenant
                if (tenant.IsVerified == true)
                {
                    return BadRequest(new VerifyTenantResponse
                    {
                        Success = false,
                        Message = "This tenant is already verified.",
                        TenantId = tenant.TenantId
                    });
                }

                // Update tenant to set IsVerified = true and clear the VerificationCode
                tenant.IsVerified = true;
                tenant.VerificationCode = null;
                await _context.SaveChangesAsync();

                // Return success response
                var response = new VerifyTenantResponse
                {
                    Success = true,
                    Message = "Tenant verified successfully.",
                    TenantId = tenant.TenantId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors
                var response = new VerifyTenantResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                };

                return StatusCode(500, response);
            }
        }
        [HttpPost("RegisterTenant")]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantRequest request)
        {
            try
            {
                // Generate a unique TenantKey and VerificationCode
                var tenantKey = Guid.NewGuid().ToString();
                var verificationCode = new Random().Next(10000, 99999).ToString();

                // Hash the password with a salt
                var salt = Guid.NewGuid().ToString();
                var passwordHash = HashPassword(request.AdminPassword, salt);

                // Insert tenant data into the database
                var tenant = new SupportTenants
                {
                    CompanyName = request.CompanyName,
                    TenantKey = tenantKey,
                    AdminEmail = request.AdminEmail,
                    IsVerified = false, // Default to not verified
                    VerificationCode = verificationCode,
                    CompanySize = request.CompanySize,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SupportTenants.Add(tenant);
                await _context.SaveChangesAsync();

                // Insert the hashed password into the TBL_MCSUPPORT_TENANT_PASSWORDS table
                var tenantPassword = new SupportTenantPassword
                {
                    TenantId = tenant.TenantId,
                    PasswordHash = passwordHash,
                    Salt = salt
                };

                _context.SupportTenantPassword.Add(tenantPassword);
                await _context.SaveChangesAsync();

                // Prepare and send the verification email
                var emailSubject = "Verify Your Company Registration";
                var emailBody = $"Thank you for registering your company. Please use the following code to verify your account: {verificationCode}";

                var emailData = new EmailData
                {
                    From = "support@marketcentral.in",
                    To = request.AdminEmail,
                    Subject = emailSubject,
                    Body = emailBody,
                    Type = "html"
                };

                // Call sendEmail function to send the invitation email
                await _emailService.SendEmailAsync(emailData);

                // Return success response with tenant details
                var response = new RegisterTenantResponse
                {
                    Success = true,
                    Message = "Tenant registered successfully. Verification code has been sent to the admin email.",
                    TenantKey = tenantKey
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors
                var response = new RegisterTenantResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                };

                return StatusCode(500, response);
            }
        }
        [HttpGet("GetFAQSectionNames")]
        public async Task<IActionResult> GetFAQSectionNames()
        {
            try
            {
                // Fetch FAQ section names excluding FAQ_SECTION_ID = 1
                var sectionNames = await _context.SupportFAQSection
                    .Where(s => s.FAQSectionId != 1)
                    .Select(s => s.FAQSectionName)
                    .ToListAsync();

                // Prepare the response
                var response = new GetFAQSectionNamesResponse
                {
                    Success = true,
                    Categories = sectionNames
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle any errors and return a generic error message
                var response = new GetFAQSectionNamesResponse
                {
                    Success = false,
                    Categories = new List<string>()
                };
                return StatusCode(500, response);
            }
        }
        [HttpPost("AutoAssignTickets")]
        public async Task<IActionResult> AutoAssignTickets()
        {
            var response = new AutoAssignResponse();
            var unassignedTickets = await _context.SupportTickets
                .Where(t => t.AssignedTo == "Unassigned" && t.Destination == "offline")
                .ToListAsync();

            var agents = await _context.SupportAgents
                .Where(a => a.Status == "offline")
                .OrderBy(a => a.AgentId)  // Sort as needed
                .ToListAsync();

            if (unassignedTickets.Count > 0 && agents.Count > 0)
            {
                foreach (var ticket in unassignedTickets)
                {
                    // Find the agent with the least number of assigned tickets
                    var leastLoadedAgent = await _context.SupportTickets
                        .Where(t => t.AssignedTo != "Unassigned")
                        .GroupBy(t => t.AssignedTo)
                        .OrderBy(g => g.Count())
                        .FirstOrDefaultAsync();

                    if (leastLoadedAgent != null)
                    {
                        var agentId = leastLoadedAgent.Key;  // Agent with the least tickets

                        // Assign the ticket to this agent
                        var ticketToAssign = await _context.SupportTickets
                            .FirstOrDefaultAsync(t => t.TicketId == ticket.TicketId && t.AssignedTo == "Unassigned");

                        if (ticketToAssign != null)
                        {
                            ticketToAssign.AssignedTo = agentId.ToString();
                            _context.SupportTickets.Update(ticketToAssign);
                        }

                        // Mark success
                        response.Status = "success";
                        response.Message = "Unassigned tickets have been successfully distributed to agents.";
                    }
                    else
                    {
                        // No suitable agent found
                        response.Status = "error";
                        response.Message = "No suitable agent found for assignment.";
                    }
                }
            }
            else
            {
                // No tickets or agents available
                response.Status = "error";
                response.Message = "No unassigned tickets or available agents found.";
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(response);
        }
        [HttpPost("ResolveTicketEmail")]
        public async Task<IActionResult> ResolveTicketEmail([FromBody] EmailData data)
        {
            var response = new { status = "error", message = "", error = "" };

            try
            {
                // Set BCC and CC if provided, else default to empty
                var bccInfo = string.IsNullOrEmpty(data.Bcc) ? null : data.Bcc;
                var ccInfo = string.IsNullOrEmpty(data.Cc) ? null : data.Cc;

                // Setup email client
                var mailMessage = new EmailData
                {
                    From = data.From,
                    Subject = data.Subject,
                    Body = data.Body,
                    Type = "html",
                    Bcc = bccInfo,
                    Cc = ccInfo
                };

                // Send the email asynchronously
                await _emailService.SendEmailAsync(mailMessage);

                // Success response
                response = new
                {
                    status = "success",
                    message = "Email sent successfully for ticket resolution.",
                    error = ""
                };
            }
            catch (Exception ex)
            {
                // Error response
                response = new
                {
                    status = "error",
                    message = "Failed to send email.",
                    error = ex.Message
                };
            }

            return Ok(response);
        }
        [HttpGet("getDetailedTicketInfo/{ticketId}")]
        public async Task<IActionResult> GetDetailedTicketInfo(int ticketId)
        {
            var response = new TicketDetailsResponse();

            try
            {
                var ticketInfo = await _context.SupportTickets.FirstOrDefaultAsync(a => a.TenantId == ticketId);

                if (ticketInfo != null)
                {
                    // Return success response with ticket details
                    response.Status = "success";
                    response.Message = "Ticket details retrieved successfully.";
                    response.Ticket = ticketInfo;
                }
                else
                {
                    // Ticket not found response
                    response.Status = "error";
                    response.Message = "Ticket not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                response.Status = "error";
                response.Message = "An error occurred while retrieving ticket details.";
                response.Error = ex.Message;
            }

            return Ok(response);
        }
        [HttpPost("UploadChatImage")]
        public async Task<IActionResult> UploadChatImage(List<IFormFile> imageFiles, [ModelBinder(BinderType = typeof(JsonModelBinder))] SupportChats request, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                // Upload Image
                var filesPaths = new List<string>();

                if (imageFiles != null && imageFiles.Any())
                {
                    foreach (var imageFile in imageFiles)
                    {
                        string folderPath = Path.Combine(env.ContentRootPath, "Photos");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var randomNum = new Random().Next(100000, 999999);
                        var newFileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + randomNum + Path.GetExtension(imageFile.FileName);

                        string filePath = Path.Combine(folderPath, newFileName);
                        using (Stream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        filesPaths.Add(filePath);
                    }
                }

                var supportChat = new SupportChats();

                supportChat.TenantId = request.TenantId;
                supportChat.AgentId = request.AgentId;
                supportChat.CustomerId = request.CustomerId;
                supportChat.Message = "Image message"; // Static message text
                supportChat.Sender = request.Sender;
                supportChat.FileUrl = filesPaths.ToString();
                supportChat.FileName = filesPaths.ToString();

                await _context.SupportChats.AddAsync(supportChat);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Chat image uploaded successfully.", FilePath = filesPaths.ToString() });


            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = "An error occurred during image upload.", Error = ex.Message });                
            }

            return Ok();
        }


    }
}
