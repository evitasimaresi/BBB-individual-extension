using BBB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBB.Data.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(64);
            builder.HasIndex(t => t.Name)
                   .IsUnique();

            builder.HasOne(t => t.TagGroup)
                   .WithMany(tg => tg.Tags)
                   .HasForeignKey(t => t.TagGroupId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
