using GameTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameTracker.Data.Services
{
    public class TagService : ITagService
    {
        private readonly GameTrackerContext _db;

        public TagService(GameTrackerContext db)
        {
            _db = db;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _db.Tags.AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task AddTagToGameAsync(int gameId, string tagName)
        {
            var normalizedName = tagName.Trim();
            if (string.IsNullOrEmpty(normalizedName)) return;

            // reuse existing tag if one with the same name already exists
            var tag = await _db.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName.ToLower());

            if (tag is null)
            {
                tag = new Tag { Name = normalizedName };
                _db.Tags.Add(tag);
                await _db.SaveChangesAsync();
            }

            // avoid creating a duplicate association if this game already has this tag
            var alreadyLinked = await _db.GameTags
                .AnyAsync(gt => gt.GameId == gameId && gt.TagId == tag.ID);

            if (!alreadyLinked)
            {
                _db.GameTags.Add(new GameTag { GameId = gameId, TagId = tag.ID });
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveTagFromGameAsync(int gameId, int tagId)
        {
            var link = await _db.GameTags
                .FirstOrDefaultAsync(gt => gt.GameId == gameId && gt.TagId == tagId);

            if (link is not null)
            {
                _db.GameTags.Remove(link);
                await _db.SaveChangesAsync();
            }
        }
    }
}
