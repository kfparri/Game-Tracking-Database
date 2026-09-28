using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GameTracker.Core.Entities;

namespace GameTracker.Data
{
    public class GameTrackerContext : DbContext
    {
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<GameTag> GameTags => Set<GameTag>();
        public DbSet<AppSettings> AppSettings => Set<AppSettings>();

        public GameTrackerContext(DbContextOptions<GameTrackerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(entity =>
            {
                entity.Property(g => g.Title).IsRequired().HasMaxLength(200);
                entity.Property(g => g.GameType).HasConversion<string>();  // readable enum in DB
                entity.HasIndex(g => g.Title);

                //entity.HasMany(g => g.Tags)
                //        .WithMany(t => t.Games)
                //        .UsingEntity(j => j.ToTable("GameTags"));  // join table
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(t => t.Name).IsUnique();  // unique tag names
            });

            modelBuilder.Entity<GameTag>()
                .HasKey(gt => new { gt.GameId, gt.TagId });

            modelBuilder.Entity<GameTag>()
                .HasOne(gt => gt.Game)
                .WithMany(g => g.GameTags)
                .HasForeignKey(gt => gt.GameId);

            modelBuilder.Entity<GameTag>()
                .HasOne(gt => gt.Tag)
                .WithMany(t => t.GameTags)
                .HasForeignKey(gt => gt.TagId);

            modelBuilder.Entity<AppSettings>()
                .HasIndex(s => s.Key)
                .IsUnique();
        }
    }
}
