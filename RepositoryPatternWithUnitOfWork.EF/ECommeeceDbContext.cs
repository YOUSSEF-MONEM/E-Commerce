using Carts.Entities;
using Categories.Entities;
using Microsoft.EntityFrameworkCore;
using Orders.Entities;
using Products.Entities;
using RepositoryPatternWithUnitOfWork.EF.Configurations;
using RepositoryPatternWithUnitOfWork.EF.ModulesConfigurations;
using RepositoryPatternWithUnitOfWork.EF.MolulesConfigurations;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Entities;

namespace RepositoryPatternWithUnitOfWork.EF
{
    public class ECommeeceDbContext : DbContext
    {
        public ECommeeceDbContext(DbContextOptions<ECommeeceDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new UserConfigurations().Configure(modelBuilder.Entity<User>());
            new UserRolesConfigurations().Configure(modelBuilder.Entity<UserRoles>());
            new ProductConfigurations().Configure(modelBuilder.Entity<Product>());
            new CategoryConfigurations().Configure(modelBuilder.Entity<Category>());
            new OrderConfigurations().Configure(modelBuilder.Entity<Order>());
            new OrderItemConfigurations().Configure(modelBuilder.Entity<OrderItem>());
            new ReviewConfigurations().Configure(modelBuilder.Entity<ProductReview>());
            new CartConfigurations().Configure(modelBuilder.Entity<Cart>());
            new CartProductConfigurations().Configure(modelBuilder.Entity<CartProduct>());
            new PaymentConfigurations().Configure(modelBuilder.Entity<Payment>());
            new ProductImageConfigurations().Configure(modelBuilder.Entity<ProductImage>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ProductReview> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartProduct> CartProducts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
