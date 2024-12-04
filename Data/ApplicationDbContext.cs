using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Varadhi.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<SupportInvitations> SupportInvitations { get; set; }
        public DbSet<SupportPasswordChangeVerification> SupportPasswordChangeVerification { get; set; }
        public DbSet<SupportRoleAssignAgents> SupportRoleAssignAgents { get; set; }
        public DbSet<SupportRoles> SupportRoles { get; set; }
        public DbSet<SupportTenantPassword> SupportTenantPassword { get; set; }
        public DbSet<SupportTenants> SupportTenants { get; set; }
        public DbSet<SupportTickets> SupportTickets { get; set; }
        public DbSet<SupportAdminsList> SupportAdminsList { get; set; }
        public DbSet<SupportAgentCustomerAssignments> SupportAgentCustomerAssignment {  get; set; }
        public DbSet<SupportAgentWorkHours> SupportAgentWorkHours { get; set; }
        public DbSet<SupportAgents> SupportAgents { get; set; }
        public DbSet<SupportChats> SupportChats { get; set; }
        public DbSet<SupportCustomers> SupportCustomers { get; set; }
        public DbSet<SupportEmailVerifications> SupportEmailVerifications { get; set; }
        public DbSet<SupportFAQSection> SupportFAQSection { get; set; }
        public DbSet<CustomerDetailInfo> CustomerDetailInfo { get; set; }
		public DbSet<SupportCustomerRating> SupportCustomerRatings { get; set; }
		public override int SaveChanges()
        {
            return base.SaveChanges();
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
