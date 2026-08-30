using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GameTracker.Core;

namespace GameTracker.Data
{
    public class GameTrackerContext : DbContext
    {
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Tag> Tags => Set<Tag>();

        public GameTrackerContext(DbContextOptions<GameTrackerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(entity =>
            {
                entity.Property(g => g.Title).IsRequired().HasMaxLength(200);
                entity.Property(g => g.Category).HasConversion<string>();  // readable enum in DB
                entity.HasIndex(g => g.Title);

                entity.HasMany(g => g.Tags)
                        .WithMany(t => t.Games)
                        .UsingEntity(j => j.ToTable("GameTags"));  // join table
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(t => t.Name).IsUnique();  // unique tag names
            });
        }
    }
}
