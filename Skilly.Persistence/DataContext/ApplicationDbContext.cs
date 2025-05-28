using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skilly.Core.Entities;
using Skilly.Persistence.Configurations;
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
        public DbSet<RequestService> requestServices { get; set; }
        public DbSet<requestServiceImage> requestServiceImages { get; set; }
        public DbSet<OfferSalary> offerSalaries { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> chats { get; set; }
        public DbSet<Banner> banners{ get; set; }
        public DbSet<Payment> payments{ get; set; }
        public DbSet<EmergencyRequest> emergencyRequests { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=GraduationProject-Skilly;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
            optionsBuilder.UseSqlServer("Server=db10869.public.databaseasp.net; Database=db10869; User Id=db10869; Password=Cq7@f4-A=H8e; Encrypt=False; MultipleActiveResultSets=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ChatConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            //modelBuilder.ApplyConfiguration(new serviceConfiguration());

        }
    }
}
