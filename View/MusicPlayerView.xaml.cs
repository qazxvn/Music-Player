using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicPlayer.ViewModel;

namespace MusicPlayer.View;

public partial class MusicPlayerView : Window
{
    public MusicPlayerView()
    {
        InitializeComponent();
        DataContext = new MusicPlayerViewModel();
    }

    public void ShowAddTrackForm_Click(object sender, RoutedEventArgs e)
    {
        AddNewTrackForm addForm = new AddNewTrackForm();
        addForm.Owner = this;
        addForm.ShowDialog();
    }

    public void CreateNewPlaylist(object sender, RoutedEventArgs e)
    {
        CreateNewPlaylistView vm = new CreateNewPlaylistView();
        vm.Owner = this;
        vm.ShowDialog();
    }

    public void AddTrackInPlaylist(object sender, RoutedEventArgs e)
    {
        AddTrackInPlaylistView vm = new AddTrackInPlaylistView();
        vm.Owner = this;
        vm.ShowDialog();
    }

    private void OnMouseCapture(object sender, MouseEventArgs e)
    {
        try
        {
            if (sender is Slider slider)
            {
                if (DataContext is MusicPlayerViewModel vm)
                {
                    vm.Rewind(slider.Value);
                }
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Не выбран трек");
        }
    }

    private void OnVolumeChanged(object sender, MouseEventArgs e)
    {
        if (sender is Slider slider)
        {
            if (DataContext is MusicPlayerViewModel vm)
            {
                vm.ChangeVolume(slider.Value);
            }
        }
    }
}