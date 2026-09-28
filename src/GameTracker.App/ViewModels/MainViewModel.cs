using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTracker.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GameTracker.App.Models.Notifications;
using Microsoft.Extensions.DependencyInjection;
using GameTracker.Data.Services;
using GameTracker.Core.Entities;
using System.ComponentModel;
using System.Windows.Data;

namespace GameTracker.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        //private readonly GameTrackerContext _db;
        private readonly IImageStorageService _imageStorageService;

        private readonly IServiceProvider _serviceProvider;
        private readonly ITagService _tagService;

        private readonly IGameService _gameService;

        public ObservableCollection<GameDetailViewModel> Games { get; } = new();

        // for the search view
        public ICollectionView GamesView { get; }

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private GameDetailViewModel? selectedGame = null;

        public ObservableCollection<ToastMessage> Toasts { get; } = new();

        public MainViewModel(IImageStorageService imageStorageService, 
                            IServiceProvider serviceProvider, 
                            ITagService tagService,
                            IGameService gameService)
        {
            //_db = db;
            _imageStorageService = imageStorageService;
            _serviceProvider = serviceProvider;
            _tagService = tagService;
            _gameService = gameService;

            _ = LoadGamesAsync();

            GamesView = CollectionViewSource.GetDefaultView(Games);
            GamesView.Filter = filterGame;
        }

        private async Task LoadGamesAsync()
        {
            //var games = await _db.Games
            //    .Include(g => g.GameTags)
            //    .ThenInclude(gt => gt.Tag)
            //    .AsNoTracking()
            //    .ToArrayAsync();
            var games = await _gameService.GetGamesAsync();

            Games.Clear();
            
            foreach (var g in games)
            {
                Games.Add(new GameDetailViewModel(g, _imageStorageService, 
                        SaveGamePlayedStateAsync, 
                        SaveGameCompleteStateAsync,
                        _tagService));
            }

            SelectedGame = null;
        }

        private async Task SaveGamePlayedStateAsync(int gameId, bool played)
        {
            //var game = await _db.Games.FindAsync(gameId);
            var game = await _gameService.GetGameAsync(gameId);

            if (game != null)
            {
                game.Played = played;
                await _gameService.UpsertAsync(game);
                //await _db.SaveChangesAsync();

                if (played)
                { 
                    _ = showToastAsync("Game Marked as Played!", ToastSeverity.Info);
                }
                else
                {
                    _ = showToastAsync("Game Marked as Not Played", ToastSeverity.Info);
                }
            }
        }

        private async Task SaveGameCompleteStateAsync(int gameId, bool completed)
        {
            //var game = await _db.Games.FindAsync(gameId);
            var game = await _gameService.GetGameAsync(gameId);

            if (game != null)
            {
                game.Completed = completed;
                await _gameService.UpsertAsync(game);
                //await _db.SaveChangesAsync();

                if (completed)
                {
                    _ = showToastAsync("Game Marked as Completed!", ToastSeverity.Info);
                }
                else
                {
                    _ = showToastAsync("Game Marked as Not Completed", ToastSeverity.Info);
                }
            }
        }

        [RelayCommand]
        private async Task SaveDetail()
        {
            bool newGame = false;

            if (SelectedGame is null) return;

            Game game;
            if (SelectedGame.Id == 0)
            {
                // new game
                newGame = true;
                game = new Game();
                //_gameService.UpsertAsync(game);
                //_db.Games.Add(game);
            }
            else
            {
                game = await _gameService.GetGameAsync(SelectedGame.Id); // _db.Games.FindAsync(SelectedGame.Id);
            }

            game.Title = SelectedGame.Title;
            game.Platform = SelectedGame.Platform;
            game.Publisher = SelectedGame.Publisher;
            game.MinPlayers = SelectedGame.MinPlayers;
            game.MaxPlayers = SelectedGame.MaxPlayers;
            game.ReleaseDate = SelectedGame.ReleaseDate;
            game.PhysicalCopy = SelectedGame.PhysicalCopy;
            game.Notes = SelectedGame.Notes;
            game.Completed = SelectedGame.Completed;
            game.PurchasedFrom = SelectedGame.PurchasedFrom;
            game.Played = SelectedGame.Played;
            game.PublisherGameID = SelectedGame.PublisherGameID;

            // save the game to get the new id
            game = await _gameService.UpsertAsync(game);
            //await _db.SaveChangesAsync();

            if (SelectedGame.PendingCoverPath is not null)
            {
                using var stream = File.OpenRead(SelectedGame.PendingCoverPath);
                var coverPath = await _imageStorageService.SaveCoverAsync(game.ID, stream, SelectedGame.PendingCoverPath);
                game.CoverImagePath = coverPath;
            }

            if (SelectedGame.PendingIconImagePath is not null)
            {
                using var stream = File.OpenRead(SelectedGame.PendingIconImagePath);
                var iconPath = await _imageStorageService.SaveIconAsync(game.ID, stream, SelectedGame.PendingIconImagePath);
                game.IconPath= iconPath;
            }

            game = await _gameService.UpsertAsync(game);

            //await _db.SaveChangesAsync();
            SelectedGame.IsEditing = false;

            if(newGame)
            {
                await showToastAsync($"Added {game.Title} to the database!", ToastSeverity.Success);
            }
            else
            {
                await showToastAsync($"Updated {game.Title}", ToastSeverity.Success);
            }

            await LoadGamesAsync();
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedGame is null) return;

            if (MessageBox.Show("Are you sure you want to delete this game?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Game game;
                game = await _gameService.GetGameAsync(SelectedGame.Id);
                //game = await _db.Games.FindAsync(SelectedGame.Id);
                //_db.Games.Remove(game);
                await _gameService.DeleteAsync(SelectedGame.Id);
                //_imageStorageService.DeleteImages(game.ID);

                //_db.SaveChanges();

                SelectedGame = null;

                _ = showToastAsync($"Deleted {game.Title} from the database", ToastSeverity.Success);

                await LoadGamesAsync();
            }
        }

        [RelayCommand]
        private void OpenDetails(GameDetailViewModel? detail)
        {
            // if the selected game is the game that is already selected, unselect the game.
            // first get that game id
            int selectedId = -1;

            if (SelectedGame is not null)
            { 
                selectedId = SelectedGame.Id;
            }

            // clear the previous selection
            clearSelection();

            if (detail is not null && detail.Id != selectedId)
            {
                SelectedGame = detail; // drives your detail flyout visibility
                SelectedGame.IsEditing = false;
                SelectedGame.IsSelected = true;
            }
            else
            {
                SelectedGame = null;
            }
        }

        [RelayCommand]
        private void NewGame()
        {
            clearSelection();

            SelectedGame = new GameDetailViewModel(null, _imageStorageService,
                        SaveGamePlayedStateAsync,
                        SaveGameCompleteStateAsync,
                        _tagService);
        }

        [RelayCommand]
        private async Task Save()
        {
            await SaveDetail();
            SelectedGame = null;
        }

        [RelayCommand]
        private async Task CancelEdit()
        {
            SelectedGame.CancelEdit();
            SelectedGame = null;
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            var settingsVm = _serviceProvider.GetRequiredService<SettingsViewModel>();
            var window = _serviceProvider.GetRequiredService<SettingsWindow>();
            window.DataContext = settingsVm;
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

            // after the dialog closes, refresh the game data
            await LoadGamesAsync();
        }

        private bool filterGame(object obj)
        {
            if (string.IsNullOrEmpty(SearchText)) return true;

            var game = (GameDetailViewModel)obj;
            var term = SearchText.Trim();

            return game.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (game.Platform?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (game.Publisher?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || game.Tags.Any(t => t.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        partial void OnSearchTextChanged(string value)
        {
            GamesView.Refresh();
        }

        private void clearSelection()
        {
            foreach (var game in Games)
            {
                game.IsSelected = false;
            }
        }

        private async Task showToastAsync(string text, ToastSeverity severity = ToastSeverity.Info)
        {
            var toast = new ToastMessage(text, severity);
            Toasts.Add(toast);

            await Task.Delay(3000); // auto-dismiss after 3s
            Toasts.Remove(toast);
        }
    }
}
