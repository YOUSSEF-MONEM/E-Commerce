using Products.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryPatternWithUnitOfWork.EF.Configurations
{
    public class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            //  Table Name
            //  Computed Column (Optional - Final Price after discount)
            // builder.Property(p => p.FinalPrice)
            //     .HasComputedColumnSql("[Price] * (1 - [DiscountPercentage])");

            //  Check Constraint (Optional - Price validation at DB level)
            //  Table Name with Check Constraints
            builder.ToTable("Products", t =>
            {
                //  Check Constraint: Price must be positive
                t.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");

                //  Check Constraint: Quantity non-negative
                t.HasCheckConstraint("CK_Products_Quantity_NonNegative", "[QuantityInStock] >= 0");

                //  Check Constraint: Discount valid range
                t.HasCheckConstraint("CK_Products_Discount_Valid",
                    "[DiscountPercentage] >= 0 AND [DiscountPercentage] <= 0.15");
            });

            //  Primary Key
            builder.HasKey(p => p.Id);

            //  Properties Configuration

            // ProductName
            builder.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            // Price
            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)") // ⬅️ 18 digits, 2 decimal places
                .HasPrecision(18, 2);

            // DiscountPercentage
            builder.Property(p => p.DiscountPercentage)
                .IsRequired()
                .HasColumnType("float")
                .HasDefaultValue(0.0); // ⬅️ Default no discount

            // QuantityInStock
            builder.Property(p => p.QuantityInStock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.SellerId)
                .IsRequired();

            // ProductImageURL
            //builder.Property(p => p.ProductImageURL)
            //    .IsRequired()
            //    .HasMaxLength(500)
            //    .HasColumnType("nvarchar(500)");

            // ProductDescription
            builder.Property(p => p.ProductDescription)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            // CategoryId
            builder.Property(p => p.CategoryId)
                .IsRequired();

            //  Relationship with Category
            builder
                .HasOne<Categories.Entities.Category>() // ⬅️ No navigation property in Product
                .WithMany() // ⬅️ Category can have many products
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // ⚠️ منع حذف Category لو فيها Products

            builder.HasOne<Users.Entities.User>()
                .WithMany() // ⬅️ Seller can have many products
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict); // ⚠️ منع حذف بائع لو لديه منتجات 

            //  Indexes for Performance
            builder.HasIndex(p => p.ProductName)
                .HasDatabaseName("IX_Products_ProductName");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            builder.HasIndex(p => p.Price)
                .HasDatabaseName("IX_Products_Price");



        }
    }
}