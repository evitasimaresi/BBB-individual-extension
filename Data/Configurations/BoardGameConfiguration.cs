using BBB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBB.Data.Configurations
{
    public class BoardGameConfiguration : IEntityTypeConfiguration<BoardGame>
    {
        public void Configure(EntityTypeBuilder<BoardGame> builder)
        {
            builder.ToTable("BoardGames");

            builder.HasKey(bg => bg.Id);

            builder.Property(bg => bg.Title)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(bg => bg.Description)
                   .HasMaxLength(2048);

            builder.Property(bg => bg.Condition)
                   .HasMaxLength(128);

            builder.Property(bg => bg.LastChange)
                   .IsRequired();

            builder.HasOne(bg => bg.Status)
                   .WithMany(s => s.BoardGames)
                   .HasForeignKey(bg => bg.StatusId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
