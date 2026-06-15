using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MusicPlayer.Model;

namespace MusicPlayer.ViewModel;

public class CreateNewPlaylistViewModel : INotifyPropertyChanged
{
    private string _playlistName;
    public string PlaylistName
    {
        get => _playlistName;
        set
        {
            _playlistName = value;
            OnPropertyChanged(nameof(PlaylistName));
        }
    }

    public CreateNewPlaylistViewModel()
    {
        CreatePlaylistCommand = new RelayCommand(CreatePlaylist);
    }

    private void CreatePlaylist()
    {
        var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var musicPlayerPath = Path.Combine(appdataPath, "MusicPlayer");
        var fullPath = Path.Combine(musicPlayerPath, "Playlists");
        
        Directory.CreateDirectory(fullPath);

        var playlistsFolderPath = Path.Combine(fullPath, PlaylistName);
        Directory.CreateDirectory(playlistsFolderPath);
        
        var jsonFilePath = Path.Combine(playlistsFolderPath, "playlist.json");

        var initialPlaylistData = new PlaylistModel
        {
            PlaylistName = PlaylistName,
            PlaylistPath = jsonFilePath,
            Tracks = new ObservableCollection<TrackModel>()
        };
        
        var jsonOptions = new JsonSerializerOptions
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        string jsonText = JsonSerializer.Serialize(initialPlaylistData, jsonOptions);
        
        File.WriteAllText(jsonFilePath, jsonText);

        var newPlaylist = new PlaylistModel
        {
            PlaylistName = PlaylistName,
            PlaylistPath = playlistsFolderPath,
            Tracks = new ObservableCollection<TrackModel>()
        };

        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<PlaylistModel>(newPlaylist));
        
        System.Windows.MessageBox.Show("Плейлист успешно создан");
    }
    
    public RelayCommand CreatePlaylistCommand { get; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}