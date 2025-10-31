using BBB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBB.Data.Configurations
{
    public class StatusConfiguration : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> builder)
        {
            builder.ToTable("Statuses");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(64);
            builder.HasIndex(s => s.Name)
                   .IsUnique();

            builder.Property(s => s.Description)
                   .IsRequired()
                   .HasMaxLength(256);
        }
    }
}
