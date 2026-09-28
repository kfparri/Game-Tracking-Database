using Microsoft.EntityFrameworkCore;
using GameTracker.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using GameTracker.Core.Entities;

var appDataRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "GameTracker");

Directory.CreateDirectory(appDataRoot);

var dbPath = Path.Combine(appDataRoot, "gameTracker.db");

var optionsBuilder = new DbContextOptionsBuilder<GameTrackerContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

using var context = new GameTrackerContext(optionsBuilder.Options);
context.Database.Migrate(); // applies pending migrations at startup

Console.WriteLine($"Database path: {dbPath}");

//var appPaths = new AppPaths(appDataRoot);
//var imageStore = new ImageStorageService(appPaths);

// add a game with an icon
var game = new Game
{
    Title = "Test Game",
    GameType = GameType.Board,
    MinPlayers = 1,
    MaxPlayers = 5
};

context.Games.Add(game);
context.SaveChanges(); // need the ID before saving the image file

await using var fs = File.OpenRead(@"C:\scripts\game-cover.png");
//game.CoverImagePath = await imageStore.SaveCoverAsync(game.ID, fs, "game-cover.png");
context.SaveChanges();

//Console.WriteLine($"Saved {game.Title} with cover image at {imageStore.ResolvePath(game.CoverImagePath)}");


