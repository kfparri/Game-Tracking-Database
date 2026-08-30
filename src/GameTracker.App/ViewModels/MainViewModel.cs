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

namespace GameTracker.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly GameTrackerContext _db;
        private readonly IImageStorageService _imageStorageService;

        public ObservableCollection<GameDetailViewModel> Games { get; } = new();

        [ObservableProperty]
        private GameDetailViewModel? selectedGame;

        public ObservableCollection<ToastMessage> Toasts { get; } = new();

        public MainViewModel(GameTrackerContext db, IImageStorageService imageStorageService)
        {
            _db = db;
            _imageStorageService = imageStorageService;
            _ = LoadGamesAsync();
        }

        private async Task LoadGamesAsync()
        {
            var games = await _db.Games.AsNoTracking().ToArrayAsync();
            Games.Clear();
            foreach (var g in games)
            {
                Games.Add(new GameDetailViewModel(g, _imageStorageService, 
                        SaveGamePlayedStateAsync, 
                        SaveGameCompleteStateAsync));
            }
        }

        private async Task SaveGamePlayedStateAsync(int gameId, bool played)
        {
            var game = await _db.Games.FindAsync(gameId);
            if (game != null)
            {
                game.Played = played;
                await _db.SaveChangesAsync();

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
            var game = await _db.Games.FindAsync(gameId);
            if (game != null)
            {
                game.Completed = completed;
                await _db.SaveChangesAsync();

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

            Core.Game game;
            if (SelectedGame.Id == 0)
            {
                // new game
                newGame = true;
                game = new Core.Game();
                _db.Games.Add(game);
            }
            else
            {
                game = await _db.Games.FindAsync(SelectedGame.Id);
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

            // save the game to get the new id
            await _db.SaveChangesAsync();

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

            await _db.SaveChangesAsync();
            SelectedGame.IsEditing = false;

            if(newGame)
            {
                _ = showToastAsync($"Added {game.Title} to the database!", ToastSeverity.Success);
            }
            else
            {
                _ = showToastAsync($"Updated {game.Title}", ToastSeverity.Success);
            }

            await LoadGamesAsync();
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedGame is null) return;

            if (MessageBox.Show("Are you sure you want to delete this game?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Core.Game game;
                game = await _db.Games.FindAsync(SelectedGame.Id);
                _db.Games.Remove(game);

                _imageStorageService.DeleteImages(game.ID);

                _db.SaveChanges();

                SelectedGame = null;

                _ = showToastAsync($"Deleted {game.Title} from the database", ToastSeverity.Success);

                await LoadGamesAsync();
            }
        }

        [RelayCommand]
        private void OpenDetails(GameDetailViewModel? detail)
        {
            // clear the previous selection
            clearSelection();

            if (detail is not null)
            {
                SelectedGame = detail; // drives your detail flyout visibility
                SelectedGame.IsEditing = false;
                SelectedGame.IsSelected = true;
            }
        }

        [RelayCommand]
        private void NewGame()
        {
            clearSelection();

            SelectedGame = new GameDetailViewModel(null, _imageStorageService,
                        SaveGamePlayedStateAsync,
                        SaveGameCompleteStateAsync);
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
