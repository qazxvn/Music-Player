using NAudio.Wave;

namespace MusicPlayer.Services;

public class AudioService
{
    private IWavePlayer _wavePlayer;
    private AudioFileReader _audioFileReader;
    public bool IsPaused { get; set; }
    public TimeSpan CurrentTrackTime { get; set; }
    public TimeSpan TrackLength { get; set; }

    public event EventHandler TrackFinished;

    public void PlayTrack(string path)
    {
        if (IsPaused && _wavePlayer != null && _audioFileReader != null)
        {
            IsPaused = false;
            _wavePlayer.Play();
            return;
        }
        else
        {
            CleanUp();
            
            _wavePlayer = new WaveOutEvent();
            _audioFileReader = new AudioFileReader(path);

            _wavePlayer.PlaybackStopped += OnPlayBackStopped;
            _wavePlayer.Init(_audioFileReader);
            _wavePlayer.Volume = 0.2f;
            TrackLength = _audioFileReader.TotalTime;
            _wavePlayer.Play();
        }
    }

    private void OnPlayBackStopped(object sender, StoppedEventArgs e)
    {
        if (IsPaused) return;
        
        if (_wavePlayer != null) _wavePlayer.PlaybackStopped -= OnPlayBackStopped;

        if (_audioFileReader != null || _audioFileReader.Position >= _audioFileReader.Length)
        {
            TrackFinished.Invoke(this, EventArgs.Empty);
        }
    }

    public void Pause()
    {
        IsPaused = true;
        CurrentTrackTime = _audioFileReader.CurrentTime;
        _wavePlayer?.Pause();
    }

    public void Stop()
    {
        _wavePlayer?.Stop();
    }

    private void CleanUp()
    {
        _wavePlayer?.Stop();
        _wavePlayer?.Dispose();
        _wavePlayer = null;
        _audioFileReader?.Dispose();
        _audioFileReader = null;
        TrackLength = TimeSpan.Zero;
    }

    public void RewindTrack(double time)
    {
        _audioFileReader.CurrentTime = TimeSpan.FromSeconds(time);
    }

    public void RewindVolume(double volume)
    {
        _wavePlayer.Volume = (float)volume;
    }
}