namespace GameTracker.Data
{
    public record AppPaths
    {
        public string RootDirectory { get; init; }

        public string DatabasePath { get; init; }

        public string ImagesDirectory { get; init; }

        public AppPaths(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            DatabasePath = Path.Combine(RootDirectory, "GameTracker.db");
            ImagesDirectory = Path.Combine(RootDirectory, "Images");
        }
    }
}
