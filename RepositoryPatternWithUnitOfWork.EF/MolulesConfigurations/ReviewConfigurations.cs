using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Configurations
{
    public class ReviewConfigurations : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            // Table name
            builder.ToTable("Reviews");

            // Primary Key
            builder.HasKey(r => r.Id);


            // Properties
            builder.Property(r => r.ProductId)
                .IsRequired();

            builder.Property(r => r.UserId)
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired(false);

            // Rating (nullable, 0-10)
            builder.Property(r => r.Rating)
                .IsRequired(false);

            // Comment (nullable)
            builder.Property(r => r.Comment)
                .HasMaxLength(1000)
                .IsRequired(false);

            // Relationships
            builder.HasOne(r => r.Product)
                .WithMany() // أو .WithMany(p => p.Reviews) لو عايز collection في Product
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // لو المنتج اتحذف، الـ Reviews تتحذف

            builder.HasOne<Users.Entities.User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // لو المستخدم اتحذف، الـ Reviews تتحذف

            // Indexes
            builder.HasIndex(r => r.ProductId)
                .HasDatabaseName("IX_Reviews_ProductId");

            builder.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_Reviews_UserId");

            builder.HasIndex(r => r.Rating)
                .HasDatabaseName("IX_Reviews_Rating");

            // Check Constraint: Rating between 0 and 10
            builder.ToTable("Reviews", t =>
            {
                t.HasCheckConstraint("CK_Reviews_Rating_Range",
                    "[Rating] IS NULL OR ([Rating] >= 0 AND [Rating] <= 10)");
            });
        }
    }
}