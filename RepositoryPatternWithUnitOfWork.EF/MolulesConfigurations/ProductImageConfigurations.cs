using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.ModulesConfigurations
{
    public class ProductImageConfigurations : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(pi => pi.Id);

            //  Properties
            builder.Property(pi => pi.ImageURL)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("ImageURL")
                .IsUnicode();

            builder.Property(pi => pi.ProductId)
                .IsRequired();

            //  Relationships
            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages) //  Assuming Product has ProductImages collection
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade); //  Delete images when product is deleted

            //  Indexes
            builder.HasIndex(pi => pi.ProductId)
                .HasDatabaseName("IX_ProductImages_ProductId");

            //  Optional: Index for faster image lookups   
            builder.HasIndex(pi => pi.ImageURL)
                .HasDatabaseName("IX_ProductImages_ImageURL");
        }
    }
}