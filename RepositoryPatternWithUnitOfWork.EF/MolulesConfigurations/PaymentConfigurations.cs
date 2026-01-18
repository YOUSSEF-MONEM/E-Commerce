using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryPatternWithUnitOfWork.EF.MolulesConfigurations
{
    public class PaymentConfigurations : IEntityTypeConfiguration<Payment>
    {

         public void Configure(EntityTypeBuilder<Payment> builder)
         {
             builder.ToTable("Payments");
             builder.HasKey(p => p.Id);

             // ✅ Properties
             builder.Property(p => p.Amount)
                 .IsRequired()
                 .HasColumnType("decimal(18,2)"); // ✅ تحديد Precision

             builder.Property(p => p.PaymentDate)
                 .HasColumnType("datetime2");

             builder.Property(p => p.PaymentStatus)
                 .IsRequired()
                 .HasConversion<int>(); // ✅ تخزين Enum كـ int

             builder.Property(p => p.PaymentMethod)
                 .HasConversion<int?>(); // ✅ Nullable Enum

             builder.Property(p => p.TransactionId)
                 .HasMaxLength(100);

             builder.Property(p => p.OrderId)
                 .IsRequired();

             builder.Property(p => p.UserId)
                 .IsRequired();

             // ✅ Relationships

             // Order Relationship (One-to-One) - Configured in Order side
             // User Relationship
             builder.HasOne<Users.Entities.User>()
                 .WithMany()
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

             // ✅ Indexes
             builder.HasIndex(p => p.OrderId)
                 .IsUnique() // ✅ كل Order له Payment واحد فقط
                 .HasDatabaseName("IX_Payments_OrderId");

             builder.HasIndex(p => p.UserId)
                 .HasDatabaseName("IX_Payments_UserId");

             builder.HasIndex(p => p.PaymentStatus)
                 .HasDatabaseName("IX_Payments_PaymentStatus");

             builder.HasIndex(p => p.TransactionId)
                 .IsUnique() // ✅ TransactionId فريد
                 .HasDatabaseName("IX_Payments_TransactionId")
                 .HasFilter("[TransactionId] IS NOT NULL"); // ✅ Unique فقط للقيم اللي مش null

             builder.HasIndex(p => p.PaymentDate)
                 .HasDatabaseName("IX_Payments_PaymentDate");

             // ✅ Composite Index
             builder.HasIndex(p => new { p.UserId, p.PaymentStatus })
                 .HasDatabaseName("IX_Payments_UserId_PaymentStatus");
         }
    }
}

