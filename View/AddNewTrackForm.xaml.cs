using System.Windows;
using Microsoft.Win32;
using MusicPlayer.ViewModel;

namespace MusicPlayer.View;

public partial class AddNewTrackForm : Window
{
    public AddNewTrackForm()
    {
        InitializeComponent();
        DataContext = new AddNewTrackFormViewModel();
    }

    private void BrowseOpen_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Title = "Выберите трек";
        openFileDialog.Filter = "Аудиофайлы (*.mp3;*.wav)|*.mp3;*.wav|Все файлы (*.*)|*.*";

        if (openFileDialog.ShowDialog() == true)
        {
            PathBox.Text = openFileDialog.FileName;
            
            if (DataContext is AddNewTrackFormViewModel vm)
            {
                vm.TrackPath = openFileDialog.FileName;
            }
        }
    }
}