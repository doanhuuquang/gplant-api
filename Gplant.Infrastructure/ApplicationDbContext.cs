using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Gplant.Domain.Entities;

namespace Gplant.Infrastructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<User> Users { set; get; }
        public DbSet<OTP> OTPs { set; get; }
        public DbSet<ActionToken> ActionTokens { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().Property(u => u.FirstName).HasMaxLength(256);
            builder.Entity<User>().Property(u => u.LastName).HasMaxLength(256);

            builder.Entity<OTP>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired();
                e.Property(x => x.Code).IsRequired();
                e.HasIndex(x => new { x.Email, x.Code });
            });

            builder.Entity<ActionToken>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.Token).IsRequired();
                e.Property(x => x.Purpose).IsRequired();
                e.HasIndex(x => new { x.UserId, x.Token, x.Purpose });
            });
        }
    }
}
