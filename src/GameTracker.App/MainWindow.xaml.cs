using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameTracker.App.ViewModels;

namespace GameTracker.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CoverDropZone_Click(object sender, MouseButtonEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.bmp;*.webp)|*.png;*.jpg;*.bmp;*.webp",
            Title = "Select cover image"
        };

        if (dialog.ShowDialog() == true)
        {
            var vm = (MainViewModel)DataContext;
            vm.SelectedGame?.SetCoverImageCommand.Execute(dialog.FileName);
        }
    }

    private void CoverDropZone_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void CoverDropZone_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        var filePath = files.FirstOrDefault();
        if (filePath is null) return;

        var vm = (MainViewModel)DataContext;
        vm.SelectedGame?.SetCoverImageCommand.Execute(filePath);
    }

    private void IconDropZone_Click(object sender, MouseButtonEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.bmp;*.webp)|*.png;*.jpg;*.bmp;*.webp",
            Title = "Select cover image"
        };

        if (dialog.ShowDialog() == true)
        {
            var vm = (MainViewModel)DataContext;
            vm.SelectedGame?.SetIconImageCommand.Execute(dialog.FileName);
        }
    }

    private void IconDropZone_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void IconDropZone_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        var filePath = files.FirstOrDefault();
        if (filePath is null) return;

        var vm = (MainViewModel)DataContext;
        vm.SelectedGame?.SetIconImageCommand.Execute(filePath);
    }
}