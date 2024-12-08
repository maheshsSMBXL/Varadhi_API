using Varadhi.Data;
using Varadhi.Models;
namespace Varadhi.Services
{
	public interface IAgentCustomerService
	{
		Task<AssignmentResponse> AssignCustomerToAgentAsync(AssignmentRequest request);
		Task<OTPResponse> ValidateOTPAsync(OTPRequest data);
		Task<LoginResponse> LoginAgentAsync(LoginRequest data);
		Task<(bool success, string message)> UpdateSocketIdAsync(string agentId, string socketId);
		Task<(bool success, string message, string customerId)> RegisterCustomerAsync(Models.CustomerRegistrationRequest request);
		Task<(bool success, string message)> UpdateCustomerSocketIdAsync(UpdateCustomerSocketRequest request);
		Task<(bool success, string message)> RaiseTicketAsync(RaiseTicketRequest request);
		Task<(bool Success, string Message, List<TicketResponse> Tickets, int TotalCount)> GetAllTicketsAsync(TicketRequest request);
		Task<(bool Success, string Message, List<TicketResponse> Tickets)> GetTicketsByCustomerIdAsync(TicketRequestCustomer request);
		Task<UpdateTicketResponse> UpdateTicketStatusAsync(UpdateTicketRequest request);
		Task<ChatHistoryResponse> GetChatHistoryAsync(ChatHistoryRequest request);
		Task<AgentCustomerResponse> GetAgentCustomerListAsync(AgentCustomerRequest request);
		Task<AgentResponse> GetAgentsAsync(AgentRequest request);
		Task<AgentResponseById> GetAgentByIdAsync(string agentId);
		Task<AssignTicketResponse> AssignTicketAsync(AssignTicketRequest request);
		Task<CustomerResponse> PostCustomerInfo(CustomerRequest request);
		Task<CustomerResponse> GetCustomerInfo(string customerId);
		Task<TicketResponseByAgentid> GetTicketsByAssignedToAsync(string agentId);
		Task<CustomerResponse> AddCustomerFeedbackAsync(CustomerFeedbackDto feedbackDto);

		//forgot password for agent
		Task<bool> IsEmailValidAsync(string email);
		Task AddForgotPwdVerificationAsync(string email, string verificationCode);
		Task<SupportAgentForgotPwdVerification> GetForgotPwdVerificationAsync(string email, string verificationCode);
		Task MarkOtpAsVerifiedAsync(int verificationId);
		Task UpdateAgentPasswordAsync(string email, string newHashedPassword);
		Task LogPasswordChangeAsync(string agentId, string changedBy, string remarks);
	}
}
