using BBB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBB.Data.Configurations
{
    public class BoardGameTagConfiguration : IEntityTypeConfiguration<BoardGameTag>
    {
        public void Configure(EntityTypeBuilder<BoardGameTag> builder)
        {
            builder.ToTable("BoardGameTags");

            builder.HasKey(bgt => new { bgt.BoardGameId, bgt.TagId });

            builder.HasOne(bgt => bgt.BoardGame)
                   .WithMany(bg => bg.BoardGameTags)
                   .HasForeignKey(bgt => bgt.BoardGameId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bgt => bgt.Tag)
                   .WithMany(t => t.BoardGameTags)
                   .HasForeignKey(bgt => bgt.TagId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
