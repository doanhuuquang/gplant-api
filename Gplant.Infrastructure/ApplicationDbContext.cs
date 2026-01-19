using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Gplant.Domain.Entities;

namespace Gplant.Infrastructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
    {
        public DbSet<OTP> OTPs { set; get; }
        public DbSet<ActionToken> ActionTokens { set; get; }
        public DbSet<Category> Categories { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(e =>
            {
                e.Property(u => u.FirstName).HasMaxLength(256);
                e.Property(u => u.LastName).HasMaxLength(256);
            });

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

            builder.Entity<Category>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
                e.Property(x => x.Slug).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.ImageUrl).HasMaxLength(500);
                e.Property(x => x.ParentId);
                e.HasIndex(x => x.Slug).IsUnique();
                e.HasIndex(x => x.ParentId);
                e.HasOne<Category>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
