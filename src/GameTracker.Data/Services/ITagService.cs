using GameTracker.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameTracker.Data.Services
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllTagsAsync();
        Task AddTagToGameAsync(int gameId, string tagName);
        Task RemoveTagFromGameAsync(int gameId, int tagId);
    }
}
