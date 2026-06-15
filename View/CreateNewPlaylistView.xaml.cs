using System.Windows;
using MusicPlayer.ViewModel;

namespace MusicPlayer.View;

public partial class CreateNewPlaylistView : Window
{
    public CreateNewPlaylistView()
    {
        InitializeComponent();
        DataContext = new CreateNewPlaylistViewModel();
    }
}