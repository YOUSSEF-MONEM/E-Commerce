using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.ModulesConfigurations
{
    // ✅ Order Configuration
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            // ✅ Properties
            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.OrderDate)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(o => o.OrderStatus)
                .IsRequired()
                .HasConversion<int>(); // ✅ تخزين Enum كـ int

            builder.Property(o => o.UserId)
                .IsRequired();

            // ✅ TotalAmount - Computed Property (Ignored)
            builder.Ignore(o => o.TotalAmount);

            // ✅ Relationships

            // User Relationship
            builder.HasOne<Users.Entities.User>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItems Relationship (One-to-Many)
            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Payment Relationship (One-to-One) - Optional
            builder.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ عشان لو حذفت Order ما يحذفش Payment تلقائيًا

            // ✅ Indexes
            builder.HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            builder.HasIndex(o => o.OrderStatus)
                .HasDatabaseName("IX_Orders_OrderStatus");

            builder.HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");

            // ✅ Composite Index للبحث المتقدم
            builder.HasIndex(o => new { o.UserId, o.OrderStatus })
                .HasDatabaseName("IX_Orders_UserId_OrderStatus");
        }
    }
}  


//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Orders.Entities;

//public class OrderConfigurations : IEntityTypeConfiguration<Order>
//{
//    public void Configure(EntityTypeBuilder<Order> builder)
//    {
//        builder.ToTable("Orders");

//        builder.HasKey(o => o.Id);

//        // Properties
//        builder.Property(o => o.ShippingAddress)
//            .IsRequired()
//            .HasMaxLength(500);

//        builder.Property(o => o.OrderDate)
//            .IsRequired();

//        builder.Property(o => o.OrderStatus)
//            .IsRequired();


//        //////سيبك من الكومبيوتد كولمن ده مع ان هو حلو بس مش لازم عشان احنا بنحسبها جوه الدومين وكمان دي بتخلي الترافك عالي على الداتا بيز فيجعل الاداء ابطأ
//        //builder.Property(o => o.TotalAmount)

//        builder.Property(o => o.UserId)
//            .IsRequired();

//        // Relationships
//        builder.HasOne<Users.Entities.User>()
//            .WithMany()
//            .HasForeignKey(o => o.UserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        builder.HasMany(o => o.OrderItems)
//            .WithOne(oi => oi.Order)
//            .HasForeignKey(oi => oi.OrderId)
//            .OnDelete(DeleteBehavior.Cascade);//يعني لو تم حذف الاوردر يتم حذف كل الاوردر ايتمز بتاعته

//        // Indexes
//        builder.HasIndex(o => o.UserId);
//        builder.HasIndex(o => o.OrderStatus);

//        builder.HasIndex(o => o.OrderDate);
//    }
//}

