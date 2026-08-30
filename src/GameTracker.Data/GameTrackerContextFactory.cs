using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameTracker.Data
{
    public class GameTrackerContextFactory : IDesignTimeDbContextFactory<GameTrackerContext>
    {
        public GameTrackerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameTrackerContext>();
            optionsBuilder.UseSqlite("Data Source=GameTracker.db");
            return new GameTrackerContext(optionsBuilder.Options);
        }
    }
}
