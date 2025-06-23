using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> modelBuilder)
        {
            modelBuilder.ToTable("Review");
            modelBuilder
            .HasOne(r => r.ServiceProvider)
            .WithMany(sp => sp.Reviews)
            .HasForeignKey(r => r.ProviderId);

        }
    }
}
