using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Gplant.Infrastructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
    {
        public DbSet<ActionToken> ActionTokens { set; get; }
        public DbSet<Banner> Banners { set; get; }
        public DbSet<CareInstruction> CareInstructions { set; get; }
        public DbSet<Cart> Carts { set; get; }
        public DbSet<CartItem> CartItems { set; get; }
        public DbSet<Category> Categories { set; get; }
        public DbSet<Folder> Folders { set; get; }
        public DbSet<Inventory> Inventories { set; get; }
        public DbSet<LightningSale> LightningSales { set; get; }
        public DbSet<LightningSaleItem> LightningSaleItems { set; get; }
        public DbSet<Media> Medias { set; get; }
        public DbSet<Order> Orders { set; get; }
        public DbSet<OrderItem> OrderItems { set; get; }
        public DbSet<OTP> OTPs { set; get; }
        public DbSet<Payment> Payments { set; get; }
        public DbSet<Plant> Plants { set; get; }
        public DbSet<PlantImage> PlantImages { set; get; }
        public DbSet<PlantVariant> PlantVariants { set; get; }
        public DbSet<ShippingAddress> ShippingAddresses { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ActionToken>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.Token).IsRequired();
                e.Property(x => x.Purpose).IsRequired();

                e.HasIndex(x => new { x.UserId, x.Token, x.Purpose });
            });

            builder.Entity<Banner>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired();
                e.Property(x => x.MediaId).IsRequired();
                e.Property(x => x.RedirectUrl).IsRequired();
                e.Property(x => x.Group).IsRequired();
                e.Property(x => x.OrderIndex).IsRequired();

                e.HasIndex(x => new { x.Id, x.Group });
                e.HasIndex(x => x.MediaId);

                e.HasOne<Media>().WithMany()
                                 .HasForeignKey(x => x.MediaId)
                                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CareInstruction>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.LightRequirement).IsRequired().HasColumnType("nvarchar(max)");
                e.Property(x => x.WateringFrequency).IsRequired().HasColumnType("nvarchar(max)");
                e.Property(x => x.Temperature).IsRequired().HasColumnType("nvarchar(max)");
                e.Property(x => x.Soil).IsRequired().HasColumnType("nvarchar(max)");
                e.Property(x => x.Notes).IsRequired().HasColumnType("nvarchar(max)");
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
            });

            builder.Entity<Cart>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.UserId).IsUnique();

                e.HasOne<User>().WithMany()
                                .HasForeignKey(x => x.UserId)
                                .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CartItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.CartId).IsRequired();
                e.Property(x => x.PlantVariantId).IsRequired();
                e.Property(x => x.Quantity).IsRequired();
                e.Property(x => x.Price).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.SalePrice).HasPrecision(18, 2);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.CartId);
                e.HasIndex(x => x.PlantVariantId);
                e.HasIndex(x => new { x.CartId, x.PlantVariantId }).IsUnique();

                e.HasOne<Cart>().WithMany()
                                .HasForeignKey(x => x.CartId)
                                .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<PlantVariant>().WithMany()
                                        .HasForeignKey(x => x.PlantVariantId)
                                        .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Category>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
                e.Property(x => x.Slug).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.MediaId);
                e.Property(x => x.ParentId);

                e.HasIndex(x => x.Slug).IsUnique();
                e.HasIndex(x => x.ParentId);
                e.HasIndex(x => x.MediaId);

                e.HasOne<Category>()
                    .WithMany()
                    .HasForeignKey(x => x.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Media>()
                    .WithMany()
                    .HasForeignKey(x => x.MediaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Folder>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
                e.Property(x => x.Slug).IsRequired().HasMaxLength(150);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()"); ;
                e.Property(x => x.MediaCount).IsRequired().HasDefaultValue(0);

                e.HasIndex(x => x.Slug).IsUnique();

                e.HasMany<Media>().WithOne()
                                  .HasForeignKey(x => x.FolderId)
                                  .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Inventory>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.PlantVariantId).IsRequired();
                e.Property(x => x.QuantityAvailable).IsRequired().HasDefaultValue(0);
                e.Property(x => x.QuantityReserved).IsRequired().HasDefaultValue(0);
                e.Property(x => x.LastUpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.PlantVariantId).IsUnique();
                e.HasIndex(x => x.QuantityAvailable);

                e.HasOne<PlantVariant>()
                    .WithMany()
                    .HasForeignKey(x => x.PlantVariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<LightningSale>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                e.Property(x => x.StartDateUtc).IsRequired();
                e.Property(x => x.EndDateUtc).IsRequired();
                e.Property(x => x.IsActive).HasDefaultValue(false);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.StartDateUtc);
                e.HasIndex(x => x.EndDateUtc);
                e.HasIndex(x => x.IsActive);
                e.HasIndex(x => new { x.IsActive, x.StartDateUtc, x.EndDateUtc });
            });

            builder.Entity<LightningSaleItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.LightningSaleId).IsRequired();
                e.Property(x => x.PlantVariantId).IsRequired();
                e.Property(x => x.OriginalPrice).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.SalePrice).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.DiscountPercentage).IsRequired().HasPrecision(5, 2);
                e.Property(x => x.QuantityLimit).IsRequired();
                e.Property(x => x.QuantitySold).HasDefaultValue(0);
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.LightningSaleId);
                e.HasIndex(x => x.PlantVariantId);
                e.HasIndex(x => new { x.LightningSaleId, x.PlantVariantId }).IsUnique();
                e.HasIndex(x => new { x.IsActive, x.QuantitySold, x.QuantityLimit });

                e.HasOne<LightningSale>()
                    .WithMany()
                    .HasForeignKey(x => x.LightningSaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<PlantVariant>()
                    .WithMany()
                    .HasForeignKey(x => x.PlantVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Media>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FileName).IsRequired().HasMaxLength(255);
                e.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
                e.Property(x => x.FileUrl).IsRequired().HasMaxLength(500);
                e.Property(x => x.FileSize).IsRequired();
                e.Property(x => x.MimeType).IsRequired().HasMaxLength(100);
                e.Property(x => x.FileHash).HasMaxLength(64);
                e.Property(x => x.FolderId).IsRequired();
                e.Property(x => x.UploadedBy).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.IsDeleted).HasDefaultValue(false);

                e.HasIndex(x => x.FileHash);
                e.HasIndex(x => x.FolderId);
                e.HasIndex(x => x.UploadedBy);
                e.HasIndex(x => x.IsDeleted);
                e.HasIndex(x => x.CreatedAtUtc);

                e.HasOne<Folder>().WithMany()
                                  .HasForeignKey(x => x.FolderId)
                                  .OnDelete(DeleteBehavior.Cascade);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            builder.Entity<Order>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.ShippingName).IsRequired().HasMaxLength(100);
                e.Property(x => x.ShippingPhone).IsRequired().HasMaxLength(20);
                e.Property(x => x.Address).IsRequired().HasMaxLength(500);
                e.Property(x => x.BuildingName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Longitude).IsRequired().HasMaxLength(100);
                e.Property(x => x.Latitude).IsRequired().HasMaxLength(100);
                e.Property(x => x.ShippingNote).HasMaxLength(500);
                e.Property(x => x.SubTotal).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.DiscountAmount).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.ShippingFee).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.Total).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.PaymentMethod).IsRequired();
                e.Property(x => x.PaymentStatus).IsRequired();
                e.Property(x => x.Status).IsRequired();
                e.Property(x => x.CancellationReason).HasMaxLength(500);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.PaymentTransactionId).HasMaxLength(100);
                e.Property(x => x.PaymentGatewayResponse).HasMaxLength(2000);

                e.HasIndex(x => x.OrderNumber).IsUnique();
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.PaymentStatus);
                e.HasIndex(x => x.CreatedAtUtc);
                e.HasIndex(x => new { x.UserId, x.Status });
                e.HasIndex(x => x.PaymentTransactionId);

                e.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.OrderId).IsRequired();
                e.Property(x => x.PlantVariantId).IsRequired();
                e.Property(x => x.PlantName).IsRequired().HasMaxLength(200);
                e.Property(x => x.VariantSKU).IsRequired().HasMaxLength(50);
                e.Property(x => x.VariantSize).IsRequired();
                e.Property(x => x.Quantity).IsRequired();
                e.Property(x => x.Price).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.SalePrice).HasPrecision(18, 2);
                e.Property(x => x.FinalPrice).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.SubTotal).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.OrderId);
                e.HasIndex(x => x.PlantVariantId);
                e.HasIndex(x => x.PlantId);

                e.HasOne<Order>().WithMany()
                                 .HasForeignKey(x => x.OrderId)
                                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<Plant>().WithMany()
                                 .HasForeignKey(x => x.PlantId)
                                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<PlantVariant>().WithMany()
                                        .HasForeignKey(x => x.PlantVariantId)
                                        .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OTP>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired();
                e.Property(x => x.Code).IsRequired();

                e.HasIndex(x => new { x.Email, x.Code });
            });

            builder.Entity<Payment>(entity =>
            {       
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.GatewayTransactionId).HasMaxLength(100);
                entity.Property(p => p.GatewayName).HasMaxLength(50);
                entity.Property(p => p.FailureReason).HasMaxLength(500);
                entity.Property(p => p.IpAddress).HasMaxLength(50);
                entity.Property(p => p.GatewayResponse).HasColumnType("nvarchar(max)");

                entity.HasOne(p => p.Order)
                      .WithMany(o => o.Payments)
                      .HasForeignKey(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.OrderId);
                entity.HasIndex(p => p.GatewayTransactionId);
            });

            builder.Entity<Plant>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Slug).IsRequired().HasMaxLength(250);
                e.Property(x => x.ShortDescription).IsRequired().HasMaxLength(500);
                e.Property(x => x.Description).IsRequired();
                e.Property(x => x.CategoryId).IsRequired();
                e.Property(x => x.CareInstructionId).IsRequired();
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.Slug).IsUnique();
                e.HasIndex(x => x.CategoryId);
                e.HasIndex(x => x.CareInstructionId);
                e.HasIndex(x => x.IsActive);

                e.HasOne<Category>()
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<CareInstruction>()
                    .WithMany()
                    .HasForeignKey(x => x.CareInstructionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlantImage>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.PlantId).IsRequired();
                e.Property(x => x.MediaId).IsRequired();
                e.Property(x => x.IsPrimary).HasDefaultValue(false);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.PlantId);
                e.HasIndex(x => x.MediaId);
                e.HasIndex(x => new { x.PlantId, x.IsPrimary });

                e.HasOne<Plant>()
                    .WithMany()
                    .HasForeignKey(x => x.PlantId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<Media>()
                    .WithMany()
                    .HasForeignKey(x => x.MediaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlantVariant>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.PlantId).IsRequired();
                e.Property(x => x.SKU).IsRequired().HasMaxLength(50);
                e.Property(x => x.Price).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.Size).IsRequired();
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.SKU).IsUnique();
                e.HasIndex(x => x.PlantId);
                e.HasIndex(x => x.IsActive);
                e.HasIndex(x => new { x.PlantId, x.IsActive });

                e.HasOne<Plant>()
                    .WithMany()
                    .HasForeignKey(x => x.PlantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ShippingAddress>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.ShippingName).IsRequired().HasMaxLength(200);
                e.Property(x => x.ShippingPhone).IsRequired().HasMaxLength(10);
                e.Property(x => x.Address).IsRequired().HasMaxLength(100);
                e.Property(x => x.BuildingName).IsRequired().HasMaxLength(100);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => x.UserId);

                e.HasOne<User>().WithMany()
                                .HasForeignKey(x => x.UserId)
                                .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<User>(e =>
            {
                e.Property(u => u.FirstName).HasMaxLength(256);
                e.Property(u => u.LastName).HasMaxLength(256);
                e.Property(u => u.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
