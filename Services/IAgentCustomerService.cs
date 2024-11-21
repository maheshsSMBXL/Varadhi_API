using Varadhi.Data;
using Varadhi.Models;
namespace Varadhi.Services
{
	public interface IAgentCustomerService
	{
		Task<AssignmentResponse> AssignCustomerToAgentAsync(AssignmentRequest request);
	}
}
