using GameTracker.Core.Settings;
using GameTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace GameTracker.Data.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly GameTrackerContext _db;

        // Keyed by the setting's string key, holding the already-deserialized value (boxed).
        // ConcurrentDictionary since settings could plausibly be read from multiple
        // async call sites around the same time (e.g. several tiles checking ShowPlayedBadge
        // during a gallery render) — avoids a race on the underlying dictionary itself.
        private readonly ConcurrentDictionary<string, object> _cache = new();

        public SettingsService(GameTrackerContext db)
        {
            _db = db;
        }

        public async Task<T> GetAsync<T>(SettingDefinition<T> definition)
        {
            if (_cache.TryGetValue(definition.Key, out var cached))
                return (T)cached!;

            var row = await _db.AppSettings.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Key == definition.Key);

            T value = row is null
                ? definition.DefaultValue
                : Deserialize<T>(row.Value);

            _cache[definition.Key] = value;
            return value;
        }

        public async Task SetAsync<T>(SettingDefinition<T> definition, T value)
        {
            var row = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == definition.Key);
            var serialized = Serialize(value);

            if (row is null)
            {
                _db.AppSettings.Add(new AppSettings { Key = definition.Key, Value = serialized });
            }
            else
            {
                row.Value = serialized;
            }

            await _db.SaveChangesAsync();

            // Update the cache only after the DB write succeeds — if SaveChangesAsync
            // throws, we don't want the cache claiming a value was saved when it wasn't
            _cache[definition.Key] = value;
        }

        private static string Serialize<T>(T value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case DateTime dt:
                    return dt.ToString("o");
                default:
                    return value.ToString() ?? string.Empty;
            }
        }

        private static T Deserialize<T>(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return default!;

            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            object converted = underlyingType switch
            {
                var t when t == typeof(DateTime) => DateTime.Parse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind),
                var t when t == typeof(bool) => bool.Parse(raw),
                var t when t == typeof(int) => int.Parse(raw),
                var t when t == typeof(string) => raw,
                _ => throw new NotSupportedException($"Type {underlyingType.Name} is not supported for deserialization.")
            };

            return (T)converted;
        }
    }
}
