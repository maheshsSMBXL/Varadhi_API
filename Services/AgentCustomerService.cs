using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Varadhi.Data;
using Varadhi.Models;
using Varadhi.Data;
namespace Varadhi.Services
{
	public class AgentCustomerService : IAgentCustomerService
	{
		private readonly ApplicationDbContext _context;

		public AgentCustomerService(ApplicationDbContext context)
		{
			_context = context;
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
	}
}
