using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTracker.Data;
using System.IO;
using System.Security.Policy;

namespace GameTracker.App.ViewModels
{
    public partial class GameDetailViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        private readonly Func<int, bool, Task> _onPlayedChanged;

        private readonly Func<int, bool, Task> _onCompleteChanged;   

        private IImageStorageService _imageStorageService;

        public int Id { get; private set; }

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        public string title = string.Empty;

        [ObservableProperty]
        public string? platform;

        [ObservableProperty]
        public string? publisher;

        [ObservableProperty]
        public int? minPlayers;

        [ObservableProperty]
        public int? maxPlayers;

        [ObservableProperty]
        public string? iconAbsolutePath;

        [ObservableProperty]
        public DateTime? releaseDate;

        [ObservableProperty]
        public string? pendingIconImagePath;

        [ObservableProperty]
        public string? coverAbsolutePath;

        [ObservableProperty]
        public string? pendingCoverPath;

        [ObservableProperty]
        public string purchasedFrom = string.Empty;

        [ObservableProperty]
        public string notes = string.Empty;

        [ObservableProperty]
        public bool played;

        [ObservableProperty]
        public bool completed;

        [ObservableProperty]
        public bool physicalCopy;

        [ObservableProperty]
        public bool isSelected;

        [ObservableProperty]
        public bool isNewEntry;

        private Core.Game _originalGame;

        public GameDetailViewModel(GameTracker.Core.Game? game, IImageStorageService imageStorageService,
            Func<int, bool, Task> onPlayedChanged, Func<int, bool, Task> onCompleteChanged)
        {
            _imageStorageService = imageStorageService;
            _originalGame = new Core.Game();

            if(game is not null)
            {
                LoadFrom(game);
                isNewEntry = false;
            }
            else
            {
                isNewEntry = true;
                isEditing = true;
            }

            _onPlayedChanged = onPlayedChanged;
            _onCompleteChanged = onCompleteChanged;

            _isInitialized = true;
        }

        public void LoadFrom(Core.Game game)
        {
            _originalGame = game;
            Id = game.ID;
            Title = game.Title;
            Platform = game.Platform;
            Publisher = game.Publisher;
            MinPlayers = game.MinPlayers;
            MaxPlayers = game.MaxPlayers;
            ReleaseDate = game.ReleaseDate;
            PhysicalCopy = game.PhysicalCopy;
            IconAbsolutePath = _imageStorageService.ResolvePath(game.IconPath) ?? string.Empty;
            CoverAbsolutePath = _imageStorageService.ResolvePath(game.CoverImagePath) ?? string.Empty;
            PurchasedFrom = game.PurchasedFrom;
            Notes = game.Notes ?? string.Empty;
            Played = game.Played;
            Completed = game.Completed;
            IsEditing = false;
        }

        [RelayCommand]
        private void SetCoverImage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext is not (".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp"))
            {
                // silently ignore non-images
                return; 
            }

            PendingCoverPath = filePath;
            CoverAbsolutePath = filePath; // preview immediately
        }

        [RelayCommand]
        private void SetIconImage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext is not (".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp"))
            {
                // silently ignore non-images
                return;
            }

            PendingIconImagePath = filePath;
            IconAbsolutePath = filePath;
        }

        [RelayCommand]
        private void BeginEdit() => IsEditing = true;

        public void CancelEdit()
        {
            if (_originalGame is not null)
            {
                LoadFrom(_originalGame);
            }
        }

        partial void OnPlayedChanged(bool oldValue, bool newValue)
        {
            if (_isInitialized)
            {
                _onPlayedChanged(Id, newValue);
            }
        }

        partial void OnCompletedChanged(bool oldValue, bool newValue)
        {
            if (_isInitialized)
            {
                _onCompleteChanged(Id, newValue);
            }
        }
    }
}
