using Varadhi.Data;

namespace Varadhi.Services
{
    public class AgentService : IAgentService
    {
        private readonly ApplicationDbContext _context;

        public AgentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAgentAsync(string agentId, string name, string email, string password, int tenantId)
        {
            var agent = new SupportAgents
            {
                AgentId = agentId,
                Name = name,
                Email = email,
                Password = password,
                TenantId = tenantId.ToString(),
                Status = "offline",
                Busy = false,
                CreatedAt = DateTime.Now
            };

            await _context.SupportAgents.AddAsync(agent);
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleToAgentAsync(string agentId, int roleId)
        {
            var roleAssignment = new SupportRoleAssignAgents
            {
                AgentID = agentId,
                RoleID = roleId
            };

            await _context.SupportRoleAssignAgents.AddAsync(roleAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task InsertEmailVerificationAsync(string email, string token, string agentId)
        {
            var verification = new SupportEmailVerifications
            {
                Email = email,
                Token = token,
                AgentId = agentId,
                CreatedAt = DateTime.Now,
                IsVerified = false
            };

            await _context.SupportEmailVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();
        }
    }
}
