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
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> modelBuilder)
        {
            modelBuilder.ToTable("Chat");
            modelBuilder
              .HasOne(c => c.FirstUser)
              .WithMany()
              .HasForeignKey(c => c.FirstUserId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .HasOne(c => c.SecondUser)
                .WithMany()
                .HasForeignKey(c => c.SecondUserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
