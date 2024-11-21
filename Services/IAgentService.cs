namespace Varadhi.Services
{
    public interface IAgentService
    {
        Task AddAgentAsync(string agentId, string name, string email, string password, int tenantId);
        Task AssignRoleToAgentAsync(string agentId, int roleId);
        Task InsertEmailVerificationAsync(string email, string token, string agentId);
    }
}
