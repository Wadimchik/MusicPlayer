using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace MusicPlayer
{
    public class MainWindow_ViewModel : INotifyPropertyChanged
    {
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyPropertyChanged();
            }
        }

        private string trackDisplayName;
        public string TrackDisplayName
        {
            get { return trackDisplayName; }
            set
            {
                trackDisplayName = value;
                NotifyPropertyChanged();
            }
        }

        private string playlistDisplayName;
        public string PlaylistDisplayName
        {
            get { return playlistDisplayName; }
            set
            {
                playlistDisplayName = value;
                NotifyPropertyChanged();
            }
        }

        private bool saveEnabled;
        public bool SaveEnabled
        {
            get { return saveEnabled; }
            set
            {
                saveEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                NotifyPropertyChanged();
            }
        }

        private Track selectedTrack;
        public Track SelectedTrack
        {
            get { return selectedTrack; }
            set
            {
                selectedTrack = value;
                try
                {
                    var file = TagLib.File.Create(value.FilePath);
                    TrackName = file.Tag.Title;
                    TrackAuthor = file.Tag.FirstArtist;
                    TrackAlbum = file.Tag.Album;
                    TrackYear = file.Tag.Year.ToString();
                }
                catch { }
                NotifyPropertyChanged();
            }
        }

        private string position;
        public string Position
        {
            get { return position; }
            set
            {
                position = value;
                NotifyPropertyChanged();
            }
        }

        private string trackName;
        public string TrackName
        {
            get { return trackName; }
            set
            {
                trackName = value;
                NotifyPropertyChanged();
            }
        }

        private string trackAuthor;
        public string TrackAuthor
        {
            get { return trackAuthor; }
            set
            {
                trackAuthor = value;
                NotifyPropertyChanged();
            }
        }

        private string trackAlbum;
        public string TrackAlbum
        {
            get { return trackAlbum; }
            set
            {
                trackAlbum = value;
                NotifyPropertyChanged();
            }
        }

        private string trackYear;
        public string TrackYear
        {
            get { return trackYear; }
            set
            {
                trackYear = value;
                NotifyPropertyChanged();
            }
        }

        public BindingList<Track> Tracks { get; set; } = new BindingList<Track>();
        private Slider slider;
        private MediaElement player;
        private Track playingTrack;
        private bool isDragging;
        private bool Repeating = false;

        private DispatcherTimer timer = new DispatcherTimer();

        private RelayCommand<object> newButtonCommand;
        public RelayCommand<object> NewButtonCommand
        {
            get
            {
                return newButtonCommand ?? (newButtonCommand = new RelayCommand<object>(obj =>
                {
                    Tracks.Clear();
                    FileName = "";
                    TrackDisplayName = "";
                    PlaylistDisplayName = "";
                    player.Stop();
                    slider.Value = 0;
                    SaveEnabled = false;
                }));
            }
        }

        private RelayCommand<object> openButtonCommand;
        public RelayCommand<object> OpenButtonCommand
        {
            get
            {
                return openButtonCommand ?? (openButtonCommand = new RelayCommand<object>(obj =>
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.DefaultExt = ".plt";
                    dialog.Filter = "Файлы плейлистов (*.plt)|*.plt";

                    Nullable<bool> result = dialog.ShowDialog();

                    if (result == true)
                    {
                        FileName = dialog.FileName;

                        FileStream stream = new FileStream(FileName, FileMode.Open);
                        XmlSerializer formatter = new XmlSerializer(typeof(Playlist));
                        Playlist playlist = formatter.Deserialize(stream) as Playlist;
                        stream.Close();

                        Tracks.Clear();
                        foreach (Track item in playlist.Tracks) Tracks.Add(item);

                        SelectedTrack = Tracks.First();
                        PlaylistDisplayName = Path.GetFileNameWithoutExtension(FileName);
                        SaveEnabled = false;
                    }
                }));
            }
        }

        private RelayCommand<object> saveButtonCommand;
        public RelayCommand<object> SaveButtonCommand
        {
            get
            {
                return saveButtonCommand ?? (saveButtonCommand = new RelayCommand<object>(obj =>
                {
                    Playlist playlist = new Playlist();
                    playlist.Tracks.AddRange(Tracks);

                    XmlSerializer formatter = new XmlSerializer(typeof(Playlist));
                    using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) formatter.Serialize(stream, playlist);

                    SaveEnabled = false;
                },
                (o) => { return SaveEnabled; }));
            }
        }

        private RelayCommand<object> saveAsButtonCommand;
        public RelayCommand<object> SaveAsButtonCommand
        {
            get
            {
                return saveAsButtonCommand ?? (saveAsButtonCommand = new RelayCommand<object>(obj =>
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.DefaultExt = ".plt";
                    dialog.Filter = "Файлы плейлистов (*.plt)|*.plt";

                    Nullable<bool> result = dialog.ShowDialog();

                    if (result == true)
                    {
                        FileName = dialog.FileName;
                        Playlist playlist = new Playlist();
                        playlist.Tracks.AddRange(Tracks);

                        XmlSerializer formatter = new XmlSerializer(typeof(Playlist));
                        using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) formatter.Serialize(stream, playlist);

                        PlaylistDisplayName = Path.GetFileNameWithoutExtension(FileName);
                        SaveEnabled = false;
                    }
                }));
            }
        }

        private RelayCommand<object> exitButtonCommand;
        public RelayCommand<object> ExitButtonCommand
        {
            get
            {
                return exitButtonCommand ?? (exitButtonCommand = new RelayCommand<object>(obj =>
                {
                    Application.Current.Shutdown();
                }));
            }
        }

        private RelayCommand<object> playButtonCommand;
        public RelayCommand<object> PlayButtonCommand
        {
            get
            {
                return playButtonCommand ?? (playButtonCommand = new RelayCommand<object>(obj =>
                {
                    try
                    {
                        if (!Repeating)
                        {
                            if (playingTrack != SelectedTrack || playingTrack == null) player.Source = new Uri(SelectedTrack.FilePath);
                            player.Play();
                            playingTrack = SelectedTrack;
                            TrackDisplayName = SelectedTrack.DisplayName;
                        }
                        else
                        {
                            if (player.Source == null)
                            {
                                player.Source = new Uri(SelectedTrack.FilePath);
                                playingTrack = SelectedTrack;
                                TrackDisplayName = SelectedTrack.DisplayName;
                            }
                            player.Stop();
                            player.Play();
                        }
                        IsPlaying = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалось воспроизвести трек " + SelectedTrack.DisplayName + ". " + ex.Message);
                    }
                },
                (o) => { return (!IsPlaying || SelectedTrack != playingTrack) && Tracks.Count > 0; }));
            }
        }

        private RelayCommand<object> pauseButtonCommand;
        public RelayCommand<object> PauseButtonCommand
        {
            get
            {
                return pauseButtonCommand ?? (pauseButtonCommand = new RelayCommand<object>(obj =>
                {
                    player.Pause();
                    IsPlaying = false;
                },
                (o) => { return IsPlaying; }));
            }
        }

        private RelayCommand<object> previousButtonCommand;
        public RelayCommand<object> PreviousButtonCommand
        {
            get
            {
                return previousButtonCommand ?? (previousButtonCommand = new RelayCommand<object>(obj =>
                {
                    int index = Tracks.IndexOf(SelectedTrack) - 1;
                    if (index >= 0 && Tracks[index] != null) SelectedTrack = Tracks[index];
                    else if (index < 0) SelectedTrack = Tracks.Last();
                    PlayButtonCommand.Execute(this);
                },
                (o) => { return Tracks.Count > 0; }));
            }
        }

        private RelayCommand<object> nextButtonCommand;
        public RelayCommand<object> NextButtonCommand
        {
            get
            {
                return nextButtonCommand ?? (nextButtonCommand = new RelayCommand<object>(obj =>
                {
                    if (!Repeating)
                    {
                        int index = Tracks.IndexOf(playingTrack) + 1;
                        int lastIndex = Tracks.Count - 1;
                        if (index <= lastIndex && Tracks[index] != null) SelectedTrack = Tracks[index];
                        else if (index > lastIndex) SelectedTrack = Tracks.First();
                    }
                    PlayButtonCommand.Execute(this);
                },
                (o) => { return Tracks.Count > 0; }));
            }
        }

        private RelayCommand<object> addButtonCommand;
        public RelayCommand<object> AddButtonCommand
        {
            get
            {
                return addButtonCommand ?? (addButtonCommand = new RelayCommand<object>(obj =>
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Multiselect = true;
                    dialog.DefaultExt = ".mp3";
                    dialog.Filter = "MP3 файлы|*.mp3";
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    Nullable<bool> result = dialog.ShowDialog();
                    if (result == true)
                    {
                        foreach (string fileName in dialog.FileNames)
                        {
                            if (Tracks.Any(x => x.DisplayName == Path.GetFileNameWithoutExtension(fileName))) continue;
                            Tracks.Add(new Track { DisplayName = Path.GetFileNameWithoutExtension(fileName), FilePath = fileName });
                        }
                        SelectedTrack = Tracks.Last();
                        SaveEnabled = true;
                    }
                }));
            }
        }

        private RelayCommand<object> removeButtonCommand;
        public RelayCommand<object> RemoveButtonCommand
        {
            get
            {
                return removeButtonCommand ?? (removeButtonCommand = new RelayCommand<object>(obj =>
                {
                    if (SelectedTrack == playingTrack)
                    {
                        player.Stop();
                        TrackDisplayName = "";
                    }
                    Tracks.Remove(SelectedTrack);
                },
                (o) => { return SelectedTrack != null; }));
            }
        }

        private RelayCommand<object> playFromStartButtonCommand;
        public RelayCommand<object> PlayFromStartButtonCommand
        {
            get
            {
                return playFromStartButtonCommand ?? (playFromStartButtonCommand = new RelayCommand<object>(obj =>
                {
                    SelectedTrack = Tracks.First();
                    PlayButtonCommand.Execute(this);
                },
                (o) => { return Tracks.Count > 0; }));
            }
        }

        private RelayCommand<object> mixButtonCommand;
        public RelayCommand<object> MixButtonCommand
        {
            get
            {
                return mixButtonCommand ?? (mixButtonCommand = new RelayCommand<object>(obj =>
                {
                    Random rng = new Random();

                    List<Track> temp = new List<Track>();
                    temp.AddRange(Tracks);
                    Tracks.Clear();

                    int n = temp.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = rng.Next(n + 1);
                        Track value = temp[k];
                        temp[k] = temp[n];
                        temp[n] = value;
                    }
                    foreach (Track item in temp) Tracks.Add(item);

                    SelectedTrack = Tracks.First();
                    PlayButtonCommand.Execute(this);
                },
                (o) => { return Tracks.Count > 0; }));
            }
        }

        private RelayCommand<object> findButtonCommand;
        public RelayCommand<object> FindButtonCommand
        {
            get
            {
                return findButtonCommand ?? (findButtonCommand = new RelayCommand<object>(obj =>
                {
                    FindWindow window = new FindWindow();
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                },
                (o) => { return Tracks.Count > 0; }));
            }
        }

        private RelayCommand<object> openTrackFolderButtonCommand;
        public RelayCommand<object> OpenTrackFolderButtonCommand
        {
            get
            {
                return openTrackFolderButtonCommand ?? (openTrackFolderButtonCommand = new RelayCommand<object>(obj =>
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = Path.GetDirectoryName(SelectedTrack.FilePath),
                        FileName = "explorer.exe"
                    };
                    Process.Start(startInfo);
                },
                (o) => { return SelectedTrack != null; }));
            }
        }

        private RelayCommand<object> repeatButtonCommand;
        public RelayCommand<object> RepeatButtonCommand
        {
            get
            {
                return repeatButtonCommand ?? (repeatButtonCommand = new RelayCommand<object>(obj =>
                {
                    Repeating = !Repeating;
                },
                (o) => { return SelectedTrack != null; }));
            }
        }

        private RelayCommand<object> listBoxDoubleClickCommand;
        public RelayCommand<object> ListBoxDoubleClickCommand
        {
            get
            {
                return listBoxDoubleClickCommand ?? (listBoxDoubleClickCommand = new RelayCommand<object>(obj =>
                {
                    PlayButtonCommand.Execute(this);
                },
                (o) => { return Tracks.Count > 0; }));
            }
        }

        private RelayCommand<RoutedPropertyChangedEventArgs<double>> sliderValueChangedCommand;
        public RelayCommand<RoutedPropertyChangedEventArgs<double>> SliderValueChangedCommand
        {
            get
            {
                return sliderValueChangedCommand ?? (sliderValueChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<double>>(OnSliderValueChanged));
            }
        }

        private RelayCommand<RoutedEventArgs> mediaElementLoadedCommand;
        public RelayCommand<RoutedEventArgs> MediaElementLoadedCommand
        {
            get
            {
                return mediaElementLoadedCommand ?? (mediaElementLoadedCommand = new RelayCommand<RoutedEventArgs>(OnMediaElementLoaded));
            }
        }

        private RelayCommand<RoutedEventArgs> sliderLoadedCommand;
        public RelayCommand<RoutedEventArgs> SliderLoadedCommand
        {
            get
            {
                return sliderLoadedCommand ?? (sliderLoadedCommand = new RelayCommand<RoutedEventArgs>(OnSliderLoaded));
            }
        }

        public MainWindow_ViewModel()
        {
            Position = "00:00:00";
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timerTick;
            timer.Start();
            Tracks.ListChanged += new ListChangedEventHandler(OnTracksListChanged);
        }

        private void OnTracksListChanged(object sender, ListChangedEventArgs e)
        {
            SaveEnabled = true;
        }

        private void timerTick(object sender, EventArgs e)
        {
            if ((player.Source != null) && (player.NaturalDuration.HasTimeSpan) && (!isDragging))
            {
                slider.Minimum = 0;
                slider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
                slider.Value = player.Position.TotalSeconds;
            }
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            slider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            NextButtonCommand.Execute(this);
        }

        private void OnMediaElementLoaded(RoutedEventArgs e)
        {
            player = e.Source as MediaElement;
            player.MediaOpened += OnMediaOpened;
            player.MediaEnded += OnMediaEnded;
        }

        private void OnSliderLoaded(RoutedEventArgs e)
        {
            slider = e.Source as Slider;
            slider.AddHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)OnDragStarted);
            slider.AddHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)OnDragCompleted);
        }

        private void OnSliderValueChanged(RoutedPropertyChangedEventArgs<double> e)
        {
            Position = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
        }

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            player.Position = TimeSpan.FromSeconds(slider.Value);
            isDragging = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
