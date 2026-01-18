using Carts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Configurations
{
    public class CartConfigurations : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            //  Primary Key
            builder.HasKey(c => c.Id);

            //  Properties
            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            //  Relationships
            builder.HasOne<Users.Entities.User>() //No navigation property in Cart
                .WithMany() // كل User ممكن يكون عنده Carts كتير
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // لو المستخدم اتحذف، الـ Cart تتحذف

            builder.HasMany(c => c.CartProducts)
                .WithOne(cp => cp.Cart)
                .HasForeignKey(cp => cp.CartId)
                .OnDelete(DeleteBehavior.Cascade); // 

            // Indexes
            builder.HasIndex(c => c.UserId)
                .IsUnique() // ⬅️ لو كل user له cart واحد بس
                .HasDatabaseName("IX_Carts_UserId_Unique");
        }
    }
}