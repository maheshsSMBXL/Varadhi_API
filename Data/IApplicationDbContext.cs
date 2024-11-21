using Microsoft.EntityFrameworkCore;

namespace Varadhi.Data
{
    public interface IApplicationDbContext
    {
        DbSet<SupportInvitation> SupportInvitations { get; set; }
        int SaveChanges();
    }
}
