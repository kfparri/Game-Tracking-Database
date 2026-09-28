using GameTracker.Core.Entities;
using GameTracker.Core.Import;
using GameTracker.Data.Services;
using System.Text.Json;

namespace GameTracker.Data.Import
{
    public class GogJsonGameImporter : IGameImporter
    {
        private readonly GameTrackerContext _db;

        private readonly IImageStorageService _imageStorageService;

        private readonly ITagService _tagService;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public ImportFormat Format => ImportFormat.GOGJson;

        public GogJsonGameImporter(GameTrackerContext db,
                                   IImageStorageService imageStorageService,
                                   ITagService tagService)
        {
            _db = db;
            _imageStorageService = imageStorageService;
            _tagService = tagService;
        }

        public async Task ImportGamesAsync(IEnumerable<ImportRecord> games, 
                                           string parentPath, 
                                           IProgress<(int current, int total)>? progress = null)
        {
            int counter = 0;

            foreach (GogImportRecord record in games)
            {
                // see if the game is already in the database
                var dbGame = _db.Games.Where(g =>
                    g.Title.ToLower() == record.name.ToLower())
                    .FirstOrDefault();

                if (dbGame is null)
                {
                    // the game does not exist in the database, create it
                    Game game = new Game()
                    {
                        Title = record.name,
                        ReleaseDate = DateTime.TryParse(record.release_date, out var releaseDate) ? releaseDate : null,
                        Publisher = record.publisher,
                        Platform = record.platforms,
                        MinPlayers = int.TryParse(record.min_players, out var minPlayers) ? minPlayers : (int?)null,
                        MaxPlayers = int.TryParse(record.max_players, out var maxPlayers) ? maxPlayers : (int?)null
                    };

                    _db.Games.Add(game);

                    await _db.SaveChangesAsync();

                    // for GOG games, both will be the logo path...
                    var imgPath = getImagePath(parentPath, record.logo);
                    await saveCoverImage(game, imgPath, record.logo);
                    await saveIconImage(game, imgPath, record.logo);

                    // gog does not have or support tags
                }
                else
                {
                    // the game already exists, update the db game
                    dbGame.ReleaseDate = DateTime.TryParse(record.release_date, out var releaseDate) ? releaseDate : null;
                    dbGame.Publisher = record.publisher;
                    dbGame.Platform = record.platforms;
                    dbGame.MinPlayers = int.TryParse(record.min_players, out var minPlayers) ? minPlayers : (int?)null;
                    dbGame.MaxPlayers = int.TryParse(record.max_players, out var maxPlayers) ? maxPlayers : (int?)null;

                    await _db.SaveChangesAsync();

                    // for GOG games, both will be the logo path...
                    var imgPath = getImagePath(parentPath, record.logo);
                    await saveCoverImage(dbGame, imgPath, record.logo);
                    await saveIconImage(dbGame, imgPath, record.logo);
                }
            }

            counter++;
            progress?.Report((counter, games.Count()));
        }

        public async Task<IEnumerable<ImportRecord>> ReadImportFileAsync(string InputPath)
        {
            if (!File.Exists(InputPath))
            {
                throw new FileNotFoundException("Import file not found", InputPath);
            }

            await using var stream = File.OpenRead(InputPath);

            var records = await JsonSerializer.DeserializeAsync<List<GogImportRecord>>(stream, _jsonSerializerOptions);

            return records ?? new List<GogImportRecord>();
        }

        private async Task saveCoverImage(Game game, string path, string imagePath)
        {
            if (File.Exists(path))
            {
                using var imageStream = File.OpenRead(path);
                var coverPath = await _imageStorageService.SaveCoverAsync(game.ID, imageStream, imagePath);
                game.CoverImagePath = coverPath;
            }
        }

        private async Task saveIconImage(Game game, string path, string imagePath)
        {
            if (File.Exists(path))
            {
                using var libraryArtStream = File.OpenRead(path);
                var iconPath = await _imageStorageService.SaveIconAsync(game.ID, libraryArtStream, imagePath);
                game.IconPath = iconPath;
            }
        }
        private async Task insertTags(string[] tags, int gameId)
        {
            foreach (var tag in tags)
            {
                await _tagService.AddTagToGameAsync(gameId, tag);
            }
        }

        private string getImagePath(string parentPath, string filePath)
        {
            string lastDirectory = Path.GetFileName(
                        parentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            string relativePath = filePath.StartsWith(lastDirectory + Path.DirectorySeparatorChar,
                                                        StringComparison.OrdinalIgnoreCase)
                    ? filePath[(lastDirectory.Length + 1)..]
                    : filePath;
            return Path.Combine(parentPath, relativePath);
        }
    }
}
