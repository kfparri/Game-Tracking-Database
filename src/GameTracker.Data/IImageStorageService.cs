using System;
using System.Collections.Generic;
using System.Text;

namespace GameTracker.Data
{
    public interface IImageStorageService
    {
        Task<string> SaveIconAsync(int gameID, Stream imageStream, string originalFileName);
        
        Task<string> SaveCoverAsync(int gameID, Stream imageStream, string originalFileName);

        string ResolvePath(string? RelativePath);

        void DeleteImages(int gameID);
    }
}
