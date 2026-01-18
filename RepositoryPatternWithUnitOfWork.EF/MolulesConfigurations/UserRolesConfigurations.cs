using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Constants;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF.ModulesConfigurations
{
    internal class UserRolesConfigurations : IEntityTypeConfiguration<UserRoles>
    {
        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            // Table Name
            builder.ToTable("UserRoles");

            // Composite Primary Key (UserId + Role)
            builder.HasKey(ur => new { ur.UserId, ur.Role });

            // UserId Configuration
            builder.Property(ur => ur.UserId)
                .IsRequired();

            // Role Configuration
            builder.Property(ur => ur.Role)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            // Relationship Configuration
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // cascade يعني لما امسح يوزر امسح كل الرولز اللي عنده 

            // Index على UserId
            builder.HasIndex(ur => ur.UserId)
                .HasDatabaseName("IX_UserRoles_UserId");
        }
    }
}