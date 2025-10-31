using BBB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBB.Data.Configurations
{
    public class TagGroupConfiguration : IEntityTypeConfiguration<TagGroup>
    {
        public void Configure(EntityTypeBuilder<TagGroup> builder)
        {
            builder.ToTable("TagGroups");

            builder.HasKey(tg => tg.Id);

            builder.Property(tg => tg.Name)
                   .IsRequired()
                   .HasMaxLength(64);
            builder.HasIndex(tg => tg.Name)
                   .IsUnique();
        }
    }
}
