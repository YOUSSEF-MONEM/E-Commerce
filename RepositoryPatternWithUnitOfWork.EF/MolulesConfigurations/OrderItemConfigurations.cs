using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.Configurations
{
    public class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            //  Composite Primary Key (OrderId + ProductId)
            builder.HasKey(oi => new { oi.OrderId, oi.ProductId });

            //  Properties
            builder.Property(oi => oi.OrderId)
                .IsRequired();

            builder.Property(oi => oi.ProductId)
                .IsRequired();

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            builder.Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

       
            //  Relationships
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);// لو حذفت الاوردر هيحذف كل الاوردر ايتمز بتوعه

            builder.HasOne<Products.Entities.Product>()
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);// مش هيسمح بحذف المنتج لو ظهر في اوردر ايتمز  

            // Indexes (Optional - for performance)
            builder.HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");

            builder.HasIndex(oi => oi.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");
        }
    }
}