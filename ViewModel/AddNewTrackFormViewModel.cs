using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MusicPlayer.Model;

namespace MusicPlayer.ViewModel;

public class AddNewTrackFormViewModel : INotifyPropertyChanged
{
    private string _newTrackName;
    public string NewTrackName
    {
        get => _newTrackName;
        set
        {
            _newTrackName = value;
            OnPropertyChanged(nameof(NewTrackName));
        }
    }

    private string _newAuthorName;

    public string NewAuthorName
    {
        get => _newAuthorName;
        set
        {
            _newAuthorName = value;
            OnPropertyChanged(nameof(NewAuthorName));
        }
    }

    private string _trackPath;

    public string TrackPath
    {
        get => _trackPath;
        set
        {
            _trackPath = value;
            OnPropertyChanged(nameof(TrackPath));
        }
    }

    public AddNewTrackFormViewModel()
    {
        AddNewTrackCommand = new RelayCommand(AddNewTrack);
    }

    private void AddNewTrack()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TrackPath) || !File.Exists(TrackPath))
            {
                throw new FileNotFoundException("Исходный файл трека не найден");
            }

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderName = "MusicPlayer";
            string fullPath = Path.Combine(appDataPath, folderName);

            Directory.CreateDirectory(fullPath);

            string extension = Path.GetExtension(TrackPath);

            string newFileName = $"{NewTrackName} ; {NewAuthorName}{extension}";
            string finalPath = Path.Combine(fullPath, newFileName);
        
            File.Move(TrackPath, finalPath, overwrite: true);

            var newTrack = new TrackModel
            {
                TrackName = NewTrackName,
                Author = NewAuthorName,
                TrackPath = finalPath
            };

            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<TrackModel>(newTrack));

            System.Windows.MessageBox.Show("Трек успешно добавлен!");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"ошибка {ex}");
        }
    }
    
    public RelayCommand AddNewTrackCommand { get; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}