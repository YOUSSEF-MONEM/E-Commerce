using Categories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.MolulesConfigurations
{
    public class CategoryConfigurations : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            //  Table Name
            builder.ToTable("Categories");
    
            //  Primary Key
            builder.HasKey(c => c.Id);
    
            //  Properties Configuration
    
            // CategoryName
            builder.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
    
            // CategoryDescription
            builder.Property(c => c.CategoryDescription)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.HasOne<User>()
                .WithMany().HasForeignKey(c => c.SellerId);
    
            // ParentCategoryId (nullable)
            builder.Property(c => c.ParentCategoryId)
                .IsRequired(false); //  Nullable for root categories
    
            //  Self-Referencing Relationship (Parent-Child)
            builder
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); //  منع الـ cascade delete
    
            //  Indexes for Performance
            builder.HasIndex(c => c.CategoryName)
                .IsUnique(); // ⬅️ Unique category names
    
            builder.HasIndex(c => c.ParentCategoryId)
                .HasDatabaseName("IX_Categories_ParentCategoryId");
    
            //  Query Filter (Soft Delete - optional)
            // builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
