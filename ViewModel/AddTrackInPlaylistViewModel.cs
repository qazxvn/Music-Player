using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MusicPlayer.Model;

namespace MusicPlayer.ViewModel;

public class AddTrackInPlaylistViewModel : INotifyPropertyChanged
{
    private string _addedTrackPath;
    public string AddedTrackPath
    {
        get => _addedTrackPath;
        set
        {
            _addedTrackPath = value;
            OnPropertyChanged(nameof(AddedTrackPath));
        }
    }

    private string _playlistPath;
    public string PlaylistPath
    {
        get => _playlistPath;
        set
        {
            _playlistPath = value;
            OnPropertyChanged(nameof(PlaylistPath));
        }
    }

    public AddTrackInPlaylistViewModel()
    {
        AddNewTrackInPlaylistCommand = new RelayCommand(AddTrackInPlaylist);
    }

    public void AddTrackInPlaylist()
    {
        var json = Directory.GetFiles(PlaylistPath, "*.json");

        var targetJsonFile = json[0];

        var jsonString = File.ReadAllText(targetJsonFile);
        var playlist = JsonSerializer.Deserialize<PlaylistModel>(jsonString);

        var trackName = Path.GetFileNameWithoutExtension(AddedTrackPath);
        var nameParts = trackName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var collection = new ObservableCollection<TrackModel>();
        collection.Add(new TrackModel
        {
            TrackName = nameParts[0],
            Author = nameParts[1],
            TrackPath = AddedTrackPath
        });
        
        playlist.Tracks.Add(new TrackModel
        {
            TrackName = nameParts[0],
            Author = nameParts[1],
            TrackPath = AddedTrackPath
        });
        
        var options = new JsonSerializerOptions { WriteIndented = true };

        var updatedJson = JsonSerializer.Serialize(playlist, options);
        File.WriteAllText(targetJsonFile, updatedJson);

        var addedInPlaylistTrack = new TrackModel
        {
            TrackName = nameParts[0],
            Author = nameParts[1],
            TrackPath = AddedTrackPath
        };

        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<TrackModel>(addedInPlaylistTrack));
    }

    public RelayCommand AddNewTrackInPlaylistCommand { get; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}