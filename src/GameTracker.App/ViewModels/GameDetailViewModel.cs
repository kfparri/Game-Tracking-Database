using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTracker.Core.Entities;
using GameTracker.Data.Services;
using System.IO;
using System.Security.Policy;
using GameTracker.Core.Entities;
using System.Collections.ObjectModel;

namespace GameTracker.App.ViewModels
{
    public partial class GameDetailViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        private readonly Func<int, bool, Task> _onPlayedChanged;

        private readonly Func<int, bool, Task> _onCompleteChanged;   

        private IImageStorageService _imageStorageService;

        private ITagService _tagService;

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
        public string? publisherGameID;

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

        private Game _originalGame;

        //[ObservableProperty]
        //private ObservableCollection<string> tags = new();

        public ObservableCollection<TagChipViewModel> Tags { get; } = new();

        [ObservableProperty]
        private string newTagText = string.Empty;

        private List<Tag> _allTagsCache = new();

        public ObservableCollection<string> TagSuggestions { get; } = new();

        [ObservableProperty]
        private bool isTagSuggestionsOpen;

        public GameDetailViewModel(Game? game, IImageStorageService imageStorageService,
            Func<int, bool, Task> onPlayedChanged, 
            Func<int, bool, Task> onCompleteChanged,
            ITagService tagService)
        {
            _imageStorageService = imageStorageService;
            _originalGame = new Game();
            _tagService = tagService;

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

        public async Task LoadFrom(Game game)
        {
            _originalGame = game;
            Id = game.ID;
            Title = game.Title;
            Platform = game.Platform;
            Publisher = game.Publisher;
            PublisherGameID = game.PublisherGameID;
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

            Tags.Clear();
            foreach (var gt in game.GameTags)
            {
                Tags.Add(new TagChipViewModel(gt.Tag.ID, gt.Tag.Name));
            }

            //Tags = new ObservableCollection<string>(game.GameTags.Select(gt => gt.Tag.Name).ToList());
        }

        partial void OnNewTagTextChanged(string value)
        {
            UpdateSuggestions(value);
        }

        private void UpdateSuggestions(string input)
        {
            TagSuggestions.Clear();

            if (string.IsNullOrWhiteSpace(input))
            {
                IsTagSuggestionsOpen = false;
                return;
            }

            var matches = _allTagsCache
                .Where(t => t.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
                .Where(t => !Tags.Any(existing => existing.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(t => t.Name)
                .Take(6)
                .Select(t => t.Name);

            foreach (var name in matches)
            {
                TagSuggestions.Add(name);
            }

            IsTagSuggestionsOpen = TagSuggestions.Count > 0;
        }

        [RelayCommand]
        private void SelectSuggestion(string tagName)
        {
            NewTagText = tagName;
            IsTagSuggestionsOpen = false;
            addTagCommand.Execute(null);
        }

        [RelayCommand]
        private void CloseSuggestions()
        {
            IsTagSuggestionsOpen = false;
        }

        public async Task LoadTagCacheAsync()
        {
            _allTagsCache = await _tagService.GetAllTagsAsync();
        }

        [RelayCommand]
        private async Task AddTag()
        {
            var name = NewTagText.Trim();
            if (string.IsNullOrEmpty(name)) return;

            // avoid adding a duplicate chip if it's already attached to this game
            if (Tags.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                newTagText = string.Empty;
                return;
            }

            await _tagService.AddTagToGameAsync(Id, name);

            // re-fetch the tag's real ID (could be a brant-new tag or an existing one)
            var allTags = await _tagService.GetAllTagsAsync();
            var tag = allTags.First(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            Tags.Add(new TagChipViewModel(tag.ID, tag.Name));
            NewTagText = string.Empty;
            IsTagSuggestionsOpen = false;

            _allTagsCache = allTags;
        }

        [RelayCommand]
        private async Task RemoveTag(TagChipViewModel chip)
        {
            await _tagService.RemoveTagFromGameAsync(Id, chip.Id);
            Tags.Remove(chip);
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
