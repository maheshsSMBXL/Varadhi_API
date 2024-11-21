using Microsoft.EntityFrameworkCore;

namespace Varadhi.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<SupportInvitation> SupportInvitations { get; set; }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
