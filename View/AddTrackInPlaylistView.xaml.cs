using System.Windows;
using Microsoft.Win32;
using MusicPlayer.ViewModel;

namespace MusicPlayer.View;

public partial class AddTrackInPlaylistView : Window
{
    public AddTrackInPlaylistView()
    {
        InitializeComponent();
        DataContext = new AddTrackInPlaylistViewModel();
    }

    public void OpenBrowseTrack(object sender, RoutedEventArgs e)
    {
        OpenFileDialog fileDialog = new OpenFileDialog();
        
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        fileDialog.InitialDirectory = appDataPath;
        
        fileDialog.Title = "Выберите трек";
        fileDialog.Filter = "Аудиофайлы (*.mp3;*.wav)|*.mp3;*.wav|Все файлы (*.*)|*.*";
        
        if (fileDialog.ShowDialog() == true)
        {
            TextBox1.Text = fileDialog.FileName;
            
            if (DataContext is AddTrackInPlaylistViewModel vm)
            {
                vm.AddedTrackPath = fileDialog.FileName;
            }
        }
    }

    public void OpenBrowsePlaylist(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        folderDialog.InitialDirectory = appDataPath;
        
        folderDialog.Title = "Выберите папку";
        
        
        if (folderDialog.ShowDialog() == true)
        {
            TextBox2.Text = folderDialog.FolderNames[0];
            
            if (DataContext is AddTrackInPlaylistViewModel vm)
            {
                vm.PlaylistPath = folderDialog.FolderName;
            }
        }
    }
}