using Microsoft.EntityFrameworkCore;

namespace Varadhi.Data
{
    public interface IApplicationDbContext
    {
        DbSet<SupportInvitations> SupportInvitations { get; set; }
        DbSet<SupportPasswordChangeVerification> SupportPasswordChangeVerification { get; set; }
        DbSet<SupportRoleAssignAgents> SupportRoleAssignAgents { get; set; }
        DbSet<SupportRoles> SupportRoles { get; set; }
        DbSet<SupportTenantPassword> SupportTenantPassword { get; set; }
        DbSet<SupportTenants> SupportTenants { get; set; }
        DbSet<SupportTickets> SupportTickets { get; set; }
        DbSet<SupportAdminsList> SupportAdminsList { get; set; }
        DbSet<SupportAgentCustomerAssignments> SupportAgentCustomerAssignment { get; set; }
        DbSet<SupportAgentWorkHours> SupportAgentWorkHours { get; set; }
        DbSet<SupportAgents> SupportAgents { get; set; }
        DbSet<SupportChats> SupportChats { get; set; }
        DbSet<SupportCustomers> SupportCustomers { get; set; }
        DbSet<SupportEmailVerifications> SupportEmailVerifications { get; set; }
        int SaveChanges();
    }
}
