using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.DataContext
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
        {
            
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> users { get; set; }
        public DbSet<UserProfile> userProfiles { get; set; }
        public DbSet<ServiceProvider> serviceProviders { get; set; }
        public DbSet<Servicesgallery> servicesgalleries { get; set; }
        public DbSet<ServicesgalleryImage>galleryImages { get; set; }
        public DbSet<ProviderServices> providerServices{ get; set; }
        public DbSet<ProviderServicesImage> providerServicesImages{ get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<Notifications> notifications { get; set; }
        public DbSet<Category> categories { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=GraduationProject-Skilly;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
            optionsBuilder.UseSqlServer("Server=db10869.public.databaseasp.net; Database=db10869; User Id=db10869; Password=Cq7@f4-A=H8e; Encrypt=False; MultipleActiveResultSets=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ServiceProvider)
                .WithMany(sp => sp.Reviews)
                .HasForeignKey(r => r.ProviderId);


        }
    }
}
