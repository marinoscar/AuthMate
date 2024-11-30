using Luval.AuthMate.Entities;
using Microsoft.EntityFrameworkCore;

namespace Luval.AuthMate
{
    public interface IAuthMateContext
    {
        DbSet<Account> Accounts { get; set; }
        DbSet<AccountType> AccountTypes { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<AppUserInAccount> UserInAccounts { get; set; }
        DbSet<AppUserRole> UserRoles { get; set; }
        DbSet<AppUser> Users { get; set; }

        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }
}