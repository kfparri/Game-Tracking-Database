using GameTracker.Core.Entities;

namespace GameTracker.Data.Services
{
    public interface IGameService
    {
        public Task<IEnumerable<Game>> GetGamesAsync();

        public Task<Game> GetGameAsync(int GameId);

        public Task<Game> UpsertAsync(Game game);

        public Task DeleteAsync(int GameId);
    }
}
