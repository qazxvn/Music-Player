using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MusicPlayer.Model;
using MusicPlayer.Services;
using Timer = System.Timers.Timer;

namespace MusicPlayer.ViewModel;

public class MusicPlayerViewModel : INotifyPropertyChanged
{
    public static ObservableCollection<TrackModel> Tracks { get; } = new();
    public ObservableCollection<PlaylistModel> Playlists { get; } = new();

    private readonly AudioService _audioService;
    private Timer _timer;
    private double _currentTimerValue;
    private int _currentIndex;
    private bool _isPlaying;
    private bool _isManualStopping;
    
    
    private PlaylistModel _selectedPlaylist;
    public PlaylistModel SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            _selectedPlaylist = value;
            OnPropertyChanged(nameof(SelectedPlaylist));
        }
    }
    

    private double _maximumTrackLength;
    public double MaximumTrackLength
    {
        get => _maximumTrackLength;
        set
        {
            _maximumTrackLength = value;
            OnPropertyChanged(nameof(MaximumTrackLength));
        }
    }

    private double _currentSliderPos;
    public double CurrentSliderPos
    {
        get => _currentSliderPos;
        set
        {
            _currentSliderPos = value;
            OnPropertyChanged(nameof(CurrentSliderPos));
        }
    }

    private double _currentVolume;
    public double CurrentVolume
    {
        get => _currentVolume;
        set
        {
            _currentVolume = value;
            OnPropertyChanged(nameof(CurrentVolume));
        }
    }

    private TrackModel _currentTrack;
    public TrackModel CurrentTrack
    {
        get => _currentTrack;
        set
        {
            _currentTrack = value;
            OnPropertyChanged(nameof(CurrentTrack));
            
            _currentIndex = Tracks.IndexOf(CurrentTrack);
            _isPlaying = true;
            _isManualStopping = true;
            CurrentSliderPos = 0;
            Play();
            MaximumTrackLength = _audioService.TrackLength.TotalSeconds;
        }
    }
    
    public MusicPlayerViewModel()
    {
        _audioService = new AudioService();
        _timer = new Timer(1000);
        _timer.Elapsed += Timer_Elapsed;
        
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<TrackModel>>(this, (r, m) =>
        {
            Tracks.Add(m.Value);
        });
        
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<PlaylistModel>>(this, (r, m) =>
        {
            Playlists.Add(m.Value);
        });
        
        _audioService.TrackFinished += AudioServiceOnTrackFinished;
        PlayTrackCommand = new RelayCommand(PlayOrStop);
        NextTrackCommand = new RelayCommand(NextTrack);
        PreviousTrackCommand = new RelayCommand(PreviousTrack);
        AddedTrackCommand = new RelayCommand(DisplayAddedTracks);
        ShowPlaylistsCommand = new RelayCommand(DisplayPlaylists);
        ShowCurrentPlaylistTracksCommand = new RelayCommand<PlaylistModel>(DisplayCurrentPlaylistTracks);
        DeletePlaylistCommand = new RelayCommand<PlaylistModel>(DeletePlaylist);
        DeleteTrackCommand = new RelayCommand<TrackModel>(DeleteTrack);
        
        DisplayPlaylists();
    }

    private void Timer_Elapsed(object? sender, EventArgs e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            if (CurrentSliderPos < MaximumTrackLength - 1)
            {
                CurrentSliderPos += 1;
                _currentTimerValue += 1;
                _isManualStopping = false;
            }
            else
            {
                StopTimer();
            }
        });
    }

    private void StopTimer()
    {
        _timer.Stop();
        _currentTimerValue = 0;
    }

    private void StartTimer()
    {
        if (CurrentSliderPos >= MaximumTrackLength) CurrentSliderPos = 0;
        _timer.Start();
    }
    
    private void AudioServiceOnTrackFinished(object? sender, EventArgs e)
    {
        if (_audioService.IsPaused || _isManualStopping)
        {
            return;
        }
        else
        {
            NextTrack();
            if(_currentTrack == null) return;
            
            _audioService.PlayTrack(_currentTrack.TrackPath);
        }
    }

    private void DeleteTrack(TrackModel trackModel)
    {
        File.Delete(trackModel.TrackPath);
        Tracks.Remove(trackModel);
    }

    private void NextTrack()
    {
        if (_currentIndex == Tracks.Count - 1)
        {
            _audioService.Stop();
            _currentTrack = null;
            _currentIndex = 0;
            return;
        }

        _audioService.IsPaused = false;
        _currentIndex += 1;
        CurrentSliderPos = 0;
        CurrentTrack = Tracks[_currentIndex];
    }

    private void PreviousTrack()
    {
        if (_currentIndex - 1 < 0)
        {
            _audioService.Stop();
            _currentTrack = null;
            _currentIndex = 0;
            return;
        }
        
        _audioService.IsPaused = false;
        _currentIndex -= 1;
        CurrentSliderPos = 0;
        CurrentTrack = Tracks[_currentIndex];
    }
    
    private void DisplayPlaylists()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var musicPlayerFolder = Path.Combine(appDataPath, "MusicPlayer");
        var playlistsFolder = Path.Combine(musicPlayerFolder, "Playlists");

        if(!Directory.Exists(playlistsFolder)) return;
        
        DirectoryInfo directoryInfo = new DirectoryInfo(playlistsFolder);

        Playlists.Clear();
        
        foreach (var playlists in directoryInfo.GetDirectories())
        {
            Playlists.Add(new PlaylistModel
            {
                PlaylistName = playlists.Name,
                PlaylistPath = playlists.FullName
            });
        }
    }
    
    private void DeletePlaylist(PlaylistModel playlistModel)
    {
        Directory.Delete(playlistModel.PlaylistPath, true);
        Playlists.Remove(playlistModel);
    }

    private void DisplayCurrentPlaylistTracks(PlaylistModel playlistModel)
    {
        var json = Directory.GetFiles(playlistModel.PlaylistPath, "*.json");

        var targetJsonFile = json[0];
        
        var jsonString = File.ReadAllText(targetJsonFile);
        var playlist = JsonSerializer.Deserialize<PlaylistModel>(jsonString);

        Tracks.Clear();
        
        foreach (var track in playlist.Tracks)
        {
            Tracks.Add(new TrackModel
            {
                TrackName = track.TrackName,
                Author = track.Author,
                TrackPath = track.TrackPath
            });
        }
    }
    
    
    private void DisplayAddedTracks()
    {
        Tracks.Clear();
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string appDataFolder = Path.Combine(appDataPath, "MusicPlayer");
        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(appDataFolder);

            foreach (var file in directoryInfo.GetFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                string[] nameParts = name.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                
                Tracks.Add(new TrackModel
                {
                    TrackName = nameParts[0],
                    Author = nameParts[1],
                    TrackPath = file.FullName
                });
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка {ex}");
        }
    }

    private void PlayOrStop()
    {
        try
        {
            if (_isPlaying)
            {
                _audioService.Pause();
                _isPlaying = false;
                _currentTimerValue = CurrentSliderPos;
                _timer.Stop();
            }
            else
            {
                _audioService.PlayTrack(CurrentTrack.TrackPath);
                _isPlaying = true;
                _timer.Start();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка {ex}");
        }
    }

    private void Play()
    {
        _audioService.PlayTrack(CurrentTrack.TrackPath);
        StartTimer();
    }

    public void Rewind(double sliderValue)
    {
        _audioService.RewindTrack(sliderValue);
    }

    public void ChangeVolume(double volume)
    {
        _audioService.RewindVolume(volume);
    }
    
    public RelayCommand<PlaylistModel> DeletePlaylistCommand { get; }
    public RelayCommand<PlaylistModel> ShowCurrentPlaylistTracksCommand { get; }
    public RelayCommand ShowPlaylistsCommand { get; }
    public RelayCommand AddedTrackCommand { get; }
    public RelayCommand PreviousTrackCommand { get; }
    public RelayCommand NextTrackCommand { get; }
    public RelayCommand PlayTrackCommand { get; }
    public RelayCommand<TrackModel> DeleteTrackCommand { get; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}