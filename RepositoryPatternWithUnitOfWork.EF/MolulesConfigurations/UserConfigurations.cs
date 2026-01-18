using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using Users.Constants;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.ModulesConfigurations
{
    internal class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table Name
            builder.ToTable("Users");

            // Primary Key
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd();//identity column



            //  تكوين RefreshTokens كـ Owned Entity
            builder.OwnsMany(u => u.RefreshTokens, rt =>
                {
                    rt.WithOwner().HasForeignKey("UserId");
                    rt.Property(t => t.Token).HasMaxLength(500); // حد أقصى للطول
                    rt.HasIndex(t => t.Token).IsUnique(); //  Token فريد
                });
            

            // FirstName Configuration
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            // LastName Configuration
            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            // Address Configuration
            builder.Property(u => u.Address)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_User_Address_MinLength",
                "LEN([Address]) >= 5"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_User_Address_Format",
                "[Address] LIKE '%-%'"
            ));

            // BirthDate Configuration
            builder.Property(u => u.BirthDate)
                .IsRequired()
                .HasColumnType("date");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_User_BirthDate_Range",
                "DATEDIFF(YEAR, [BirthDate], GETDATE()) BETWEEN 20 AND 60"
            ));

            // Email Configuration
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email_Unique");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_User_Email_Format",
                "[Email] LIKE '%@%.%'"
            ));

            // Password Configuration
            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_User_Password_MinLength",
                "LEN([Password]) >= 8"
            ));

            // CreatedAt Configuration
            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2");

            // UpdatedAt Configuration
            builder.Property(u => u.UpdatedAt)
                .HasColumnType("datetime2");


            // Unique constraint على PhoneNumber (optional)
            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_User_PhoneNumber_Unique");



            // Navigation Property للـ Roles
            builder.HasMany(u => u.Roles)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);//يعني لما تحذف اليوزر ، تحذف كل الرولز المرتبطة بيه كمان يعني الداتابيز مش هتحزرني

            //Indexes لتحسين أداء الاستعلامات
            builder.HasIndex(u => u.LastName)
                .HasDatabaseName("IX_User_LastName");
        }
    }
}