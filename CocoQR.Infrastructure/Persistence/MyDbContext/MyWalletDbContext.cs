using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CocoQR.Infrastructure.Persistence.MyDbContext
{
    public partial class CocoQRDbContext : DbContext
    {
        public CocoQRDbContext() { }

        public CocoQRDbContext(DbContextOptions<CocoQRDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<QRHistory> QRHistories { get; set; }
        public DbSet<QRStyle> QRStyles { get; set; }
        public DbSet<QRStyleLibrary> QRStyleLibraries { get; set; }
        public DbSet<BankInfo> BankInfos { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<SmtpSetting> SmtpSettings { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CocoQRDbContext).Assembly);

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
