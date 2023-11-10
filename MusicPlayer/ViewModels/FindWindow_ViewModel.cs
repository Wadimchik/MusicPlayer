using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayer
{
    public class FindWindow_ViewModel : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        private bool findEnabled;
        public bool FindEnabled
        {
            get { return findEnabled; }
            set
            {
                findEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand<Window> findButtonCommand;
        public RelayCommand<Window> FindButtonCommand
        {
            get
            {
                return findButtonCommand ?? (findButtonCommand = new RelayCommand<Window>(OnFindButtonClicked, (o) => { return FindEnabled; } ));
            }
        }

        private RelayCommand<TextChangedEventArgs> textChangedCommand;
        public RelayCommand<TextChangedEventArgs> TextChangedCommand
        {
            get
            {
                return textChangedCommand ?? (textChangedCommand = new RelayCommand<TextChangedEventArgs>(OnTextChanged));
            }
        }

        private void OnFindButtonClicked(Window window)
        {
            MainWindow_ViewModel mainWindowVM = Application.Current.MainWindow.DataContext as MainWindow_ViewModel;
            Track desired = null;
            foreach (Track item in mainWindowVM.Tracks)
            {
                if (item.DisplayName.Contains(Name))
                {
                    desired = item;
                    break;
                }
            }
            if (desired != null)
            {
                mainWindowVM.SelectedTrack = desired;
                if (window != null) window.Close();
            }
            else MessageBox.Show("Трек, содержащий в имени \"" + Name + "\" не найден в открытом плейлисте.");
        }

        private void OnTextChanged(TextChangedEventArgs e)
        {
            TextBox textBox = e.Source as TextBox;
            if (textBox != null)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text) && !string.IsNullOrEmpty(textBox.Text)) FindEnabled = true;
                else FindEnabled = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
