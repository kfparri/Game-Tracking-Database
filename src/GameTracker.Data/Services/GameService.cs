using GameTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameTracker.Data.Services
{
    public class GameService : IGameService
    {
        private readonly GameTrackerContext _db;

        public GameService(GameTrackerContext db)
        {
            _db = db;
        }

        public async Task DeleteAsync(int GameId)
        {
            var game = await _db.Games.FindAsync(GameId);
            _db.Games.Remove(game);
            await _db.SaveChangesAsync();
        }

        public async Task<Game> GetGameAsync(int GameId)
        {
            return await _db.Games.FindAsync(GameId);
        }

        public async Task<IEnumerable<Game>> GetGamesAsync()
        {
            return await _db.Games
                .Include(g => g.GameTags)
                .ThenInclude(gt => gt.Tag)
                .AsNoTracking()
                .OrderBy(g => g.Title)
                .ToListAsync();
        }      

        public async Task<Game> UpsertAsync(Game game)
        {
            Game temp;

            if (game.ID > 0)
            {
                // perform an update to the game
                temp = await _db.Games.FindAsync(game.ID);

                if (temp is not null)
                {
                    temp.Title = game.Title;
                    temp.GameType = game.GameType;
                    temp.Platform = game.Platform;
                    temp.Publisher = game.Publisher;
                    temp.PublisherGameID = game.PublisherGameID;
                    temp.MinPlayers = game.MinPlayers;
                    temp.MaxPlayers = game.MaxPlayers;
                    temp.ReleaseDate = game.ReleaseDate;
                    temp.Notes = game.Notes;
                    temp.Played = game.Played;
                    temp.Completed = game.Completed;
                    temp.PurchasedFrom = game.PurchasedFrom;
                    temp.PhysicalCopy = game.PhysicalCopy;
                    temp.IconPath = game.IconPath;
                    temp.CoverImagePath = game.CoverImagePath;
                    temp.GameTags = game.GameTags;
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            else
            {
                // new game
                _db.Games.Add(game);
            }

            await _db.SaveChangesAsync();

            return game;
        }
    }
}
