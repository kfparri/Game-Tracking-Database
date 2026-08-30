using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameTracker.Data
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly string _rootPath;
        private readonly string _imageFolderPath;

        public ImageStorageService(string appDataRoot)
        {
            _rootPath = appDataRoot;
            _imageFolderPath = Path.Combine(_rootPath, "images");

            Directory.CreateDirectory(_imageFolderPath);
            //Directory.CreateDirectory(Path.Combine(_rootPath, "icons"));
            //Directory.CreateDirectory(Path.Combine(_rootPath, "covers"));
        }

        public async Task<string> SaveIconAsync(int gameID, Stream imageStream, string originalFileName)
            => await SaveAsync("icon", gameID, imageStream, originalFileName);

        public async Task<string> SaveCoverAsync(int gameID, Stream imageStream, string originalFileName)
            => await SaveAsync("cover", gameID, imageStream, originalFileName);

        public async Task<string> SaveAsync(string imageType, int gameID, Stream imageStream, string originalFileName)
        {
            var fileExtension = Path.GetExtension(originalFileName);

            if(string.IsNullOrWhiteSpace(fileExtension))
            {
                fileExtension = ".png"; // Default to .png if no extension is provided
            }

            var fileName = "";

            if (imageType == "icon")
            {
                fileName = $"icon.{fileExtension}";
            }
            else
            {
                fileName = $"cover.{fileExtension}";
            }

            //var fileName = $"{gameID}{fileExtension}";
            Directory.CreateDirectory(Path.Combine(_imageFolderPath, gameID.ToString()));

            var filePath = Path.Combine(_imageFolderPath, gameID.ToString(), fileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            return Path.Combine("images", gameID.ToString(), fileName).Replace('\\', '/');
        }

        public string ResolvePath(string? relativePath)
        {
            if (relativePath is not null)
            {
                return Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            }
            else
            {
                return "";
            }
        }

        public void DeleteImages(int gameID)
        {
            var iconPath = Path.Combine(_imageFolderPath, gameID.ToString(), "icon.*");
            var coverPath = Path.Combine(_imageFolderPath, gameID.ToString(), "cover.*");

            foreach (var file in Directory.GetFiles(Path.Combine(_imageFolderPath, gameID.ToString()))) //, "icon.*"))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(Path.Combine(_imageFolderPath, gameID.ToString()))) //, "cover.*"))
            {
                File.Delete(file);
            }

            Directory.Delete(Path.Combine(_imageFolderPath, gameID.ToString()));
        }
    }
}
