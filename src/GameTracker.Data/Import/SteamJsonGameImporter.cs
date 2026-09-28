using GameTracker.Core;
using GameTracker.Core.Entities;
using GameTracker.Core.Import;
using GameTracker.Data.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameTracker.Data.Import
{
    public class SteamJsonGameImporter : IGameImporter
    {
        private readonly GameTrackerContext _db;
        private readonly IImageStorageService _imageStorageService;
        private readonly ITagService _tagService;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public SteamJsonGameImporter(GameTrackerContext db,
                                     IImageStorageService imageStorageService,
                                     ITagService tagService)
        {
            _db = db;
            _imageStorageService = imageStorageService;
            _tagService = tagService;
        }

        public ImportFormat Format => ImportFormat.SteamJson;

        public async Task ImportGamesAsync(IEnumerable<ImportRecord> games, 
                                           string parentPath, 
                                           IProgress<(int current, int total)>? progress = null)
        {
            int counter = 0;

            foreach (SteamImportRecord record in games)
            {
                var dbGame = _db.Games.Where(g =>
                    g.Title.ToLower() == record.name.ToLower())
                    .FirstOrDefault();

                if (dbGame is null)
                {
                    // create a new game record.
                    Game game = new Game()
                    {
                        Title = record.name,
                        GameType = GameType.Video,
                        Platform = record.platforms,
                        Publisher = record.publisher,
                        PublisherGameID = record.appid.ToString(),
                        MinPlayers = int.TryParse(record.min_players, out var minPlayers) ? minPlayers : (int?)null,
                        MaxPlayers = int.TryParse(record.max_players, out var maxPlayers) ? maxPlayers : (int?)null,
                        ReleaseDate = DateTime.TryParse(record.release_date, out var releaseDate) ? releaseDate : null,
                        Played = record.playtime_minutes > 0,
                        Completed = false,
                        PurchasedFrom = "Steam",
                        PhysicalCopy = false,
                    };

                    _db.Games.Add(game);

                    await _db.SaveChangesAsync();

                    var headerPath = getImagePath(parentPath, record.header);
                    await saveCoverImage(game, headerPath, record.header);

                    var libraryPath = getImagePath(parentPath, record.library_art); // Path.Combine(parentPath, record.library_art);
                    await saveIconImage(game, libraryPath, record.library_art);

                    await _db.SaveChangesAsync();

                    // add tags
                    if (record.tags.Length > 0)
                    {
                        var tags = record.tags.Split(';');

                        await insertTags(tags, game.ID);
                    }

                }
                else
                {
                    // the game already exists, update the db game
                    dbGame.Platform = record.platforms;
                    dbGame.Publisher = record.publisher;
                    dbGame.MinPlayers = int.TryParse(record.min_players, out var minPlayers) ? minPlayers : (int?)null;
                    dbGame.MaxPlayers = int.TryParse(record.max_players, out var maxPlayers) ? maxPlayers : (int?)null;
                    dbGame.Played = record.playtime_minutes > 0;

                    // add images
                    var headerPath = getImagePath(parentPath, record.header); // Path.Combine(parentPath, record.header);

                    await _db.SaveChangesAsync();

                    await saveCoverImage(dbGame, headerPath, record.header);

                    var libraryPath = getImagePath(parentPath, record.library_art); // Path.Combine(parentPath, record.library_art);
                    await saveCoverImage(dbGame, libraryPath, record.library_art);

                    await _db.SaveChangesAsync();

                    if (record.tags.Length > 0)
                    {
                        var tags = record.tags.Split(';');

                        await insertTags(tags, dbGame.ID);
                    }

                }

                counter++;
                progress?.Report((counter, games.Count()));
            }
        }

        public async Task<IEnumerable<ImportRecord>> ReadImportFileAsync(string InputPath)
        {
            if (!File.Exists(InputPath))
            {
                throw new FileNotFoundException("Import file not found,", InputPath);
            }

            // read json file
            await using var stream = File.OpenRead(InputPath);

            // parse the records into entities and save them using existing services
            var records = await JsonSerializer.DeserializeAsync<List<SteamImportRecord>>(stream, _jsonSerializerOptions);

            return records ?? new List<SteamImportRecord>();
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
