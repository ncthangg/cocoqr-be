using Microsoft.EntityFrameworkCore;
using MyWallet.Domain.Entities;

namespace MyWallet.Infrastructure.Persistence.MyDbContext
{
    public partial class MyWalletDbContext : DbContext
    {
        public MyWalletDbContext() { }

        public MyWalletDbContext(DbContextOptions<MyWalletDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<QRHistory> QRHistories { get; set; }
        public DbSet<BankInfo> BankInfos { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyWalletDbContext).Assembly);

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
