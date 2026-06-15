using System.Collections.ObjectModel;

namespace MusicPlayer.Model;

public class PlaylistModel
{
    public string PlaylistName { get; set; }
    public string PlaylistPath { get; set; }
    public ObservableCollection<TrackModel> Tracks { get; set; }
}