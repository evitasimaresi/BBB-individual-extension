using BBB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBB.Data.Configurations
{
    public class BoardGameUserConfiguration : IEntityTypeConfiguration<BoardGameUser>
    {
        public void Configure(EntityTypeBuilder<BoardGameUser> builder)
        {
            builder.ToTable("BoardGameUsers");

            builder.HasKey(bgu => bgu.Id);

            builder.HasOne(bgu => bgu.BoardGame)
                   .WithMany(bg => bg.BoardGameUsers)
                   .HasForeignKey(bgu => bgu.BoardGameId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bgu => bgu.User)
                   .WithMany(u => u.BoardGameUsers)
                   .HasForeignKey(bgu => bgu.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(bgu => bgu.BorrowDate)
                   .IsRequired();
        }
    }
}
