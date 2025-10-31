using BBB.Data.Configurations;
using BBB.Models;
using Microsoft.EntityFrameworkCore;

namespace BBB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Core entities
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Auth> Auths => Set<Auth>();

        public DbSet<BoardGame> BoardGames => Set<BoardGame>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<TagGroup> TagGroups => Set<TagGroup>();

        // Join tables
        public DbSet<BoardGameUser> BoardGameUsers => Set<BoardGameUser>();
        public DbSet<BoardGameTag> BoardGameTags => Set<BoardGameTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply configurations
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AuthConfiguration());
            modelBuilder.ApplyConfiguration(new BoardGameConfiguration());
            modelBuilder.ApplyConfiguration(new StatusConfiguration());
            modelBuilder.ApplyConfiguration(new TagGroupConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
            modelBuilder.ApplyConfiguration(new BoardGameUserConfiguration());
            modelBuilder.ApplyConfiguration(new BoardGameTagConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
