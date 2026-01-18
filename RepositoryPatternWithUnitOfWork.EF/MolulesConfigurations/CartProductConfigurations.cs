using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Carts.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Configurations
{
    public class CartProductConfigurations : IEntityTypeConfiguration<CartProduct>
    {
        public void Configure(EntityTypeBuilder<CartProduct> builder)
        {
            builder.ToTable("CartProducts");

            // Composite Primary Key (CartId + ProductId)
            builder.HasKey(cp => new { cp.CartId, cp.ProductId });

            // Properties
            builder.Property(cp => cp.CartId)
                .IsRequired();

            builder.Property(cp => cp.ProductId)
                .IsRequired();

            builder.Property(cp => cp.Quantity)
                .IsRequired();

            builder.Property(cp => cp.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(cp => cp.AddedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(cp => cp.UpdatedAt)
                .IsRequired(false);

            // Computed property (ignored in DB)الافضل من ناحية الاداء الن ده معتمد على بروبرتي اخري فيتحسب في الرن تايم ولو عايز تجيبه من الداتابيز عادي جدا بجملة سليكت
            //builder.Property(oi => oi.Total)
            //     .HasComputedColumnSql(sql: "[Quantity] * [UnitPrice]", stored: true);
            // builder.Ignore(cp => cp.LineTotal);

            // Relationships
            builder.HasOne(cp => cp.Cart)
                .WithMany(c => c.CartProducts)
                .HasForeignKey(cp => cp.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Products.Entities.Product>()
                .WithMany()
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // منع حذف المنتج لو موجود في Carts

            // Indexes
            builder.HasIndex(cp => cp.CartId)
                .HasDatabaseName("IX_CartProducts_CartId");

            builder.HasIndex(cp => cp.ProductId)
                .HasDatabaseName("IX_CartProducts_ProductId");

            // Check Constraints
            builder.ToTable("CartProducts", t =>
            {
                t.HasCheckConstraint("CK_CartProducts_Quantity_Positive",
                    "[Quantity] > 0");

                t.HasCheckConstraint("CK_CartProducts_UnitPrice_Positive",
                    "[UnitPrice] > 0");
            });
        }
    }
}