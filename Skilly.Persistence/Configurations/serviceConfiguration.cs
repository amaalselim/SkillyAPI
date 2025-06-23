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
    public class serviceConfiguration : IEntityTypeConfiguration<ProviderServices>
    {
        public void Configure(EntityTypeBuilder<ProviderServices> modelBuilder)
        {
            modelBuilder.ToTable("ProviderServices");
            modelBuilder
                    .Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

            modelBuilder
                .Property(p => p.PriceDiscount)
                .HasColumnType("decimal(18,2)");

        }
    }
}
