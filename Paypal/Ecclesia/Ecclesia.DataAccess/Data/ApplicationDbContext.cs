using Ecclesia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecclesia.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Oil", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Acrylic", DisplayOrder = 2 },
                new Category { Id = 3, Name = "WaterColour", DisplayOrder = 3 }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Title = "Sea", Description = "Abstract painting of whales majestically swimming over the somewhat rough ocean. The light of the sun gleaming through the cloud pale blue sky", ISBN = "SOTJ11288733", ListPrice = 12000, CategoryId = 3, ImageUrl = "/images/sea.jpg" },
                new Product { Id = 2, Title = "River", Description = "Valley river bed, with flowing river, littered with fallen and weathered rocks from the valley above", ISBN = "EWBR3956683", ListPrice = 180000, CategoryId = 1, ImageUrl = "/images/river.jpg" },
                new Product { Id = 3, Title = "Field", Description = "Farm field, with classic car depicting a scene from the 1940s", ISBN = "SOTJ11288733", ListPrice = 40000, CategoryId = 2, ImageUrl = "/images/field.jpg" }
            );
        }
    }
}
