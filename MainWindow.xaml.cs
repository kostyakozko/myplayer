using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyPlayer;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Collections.ObjectModel;

namespace myplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> Cases { get; set; }

        public ObservableCollection<SongDbItems> Music { get; set; }

        public double ProgressValue {
            get 
            {
                return progressValue;
            }
            set
            {
                progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }

        public System.Windows.Shell.TaskbarItemProgressState ProgressState
        {
            get
            {
                return progressState;
            }
            set
            {
                progressState = value;
                OnPropertyChanged("ProgressState");
            }
        }

        public System.Windows.Visibility PlayVisibility
        {
            get
            {
                return playVisibility;
            }
            set
            {
                playVisibility = value;
                OnPropertyChanged("PlayVisibility");
            }
        }

        public System.Windows.Visibility PauseVisibility
        {
            get
            {
                return pauseVisibility;
            }
            set
            {
                pauseVisibility = value;
                OnPropertyChanged("PauseVisibility");
            }
        }

        public bool PlayEnabled
        {
            get
            {
                return playEnabled;
            }
            set
            {
                playEnabled = value;
                OnPropertyChanged("PlayEnabled");
            }
        }

        public bool PauseEnabled
        {
            get
            {
                return pauseEnabled;
            }
            set
            {
                pauseEnabled = value;
                OnPropertyChanged("PauseEnabled");
            }
        }

        public string PlayPauseButtonSource
        {
            get
            {
                return playPauseButtonSource;
            }

            set
            {
                playPauseButtonSource = value;
                OnPropertyChanged("PlayPauseButtonSource");
            }
        }

        public bool ShuffleSongs
        {
            get
            {
                return shuffleSongs;
            }

            set
            {
                shuffleSongs = value;
            }
        }
        private string dirpath;
        private bool shuffleSongs = false;
        private double progressValue;
        private ObservableCollection<SongDbItems> music;
        private System.Windows.Shell.TaskbarItemProgressState progressState;
        private System.Windows.Visibility playVisibility;
        private System.Windows.Visibility pauseVisibility;
        private bool playEnabled;
        private bool pauseEnabled;
        private string playPauseButtonSource;
        private string filename;
        private string filter = "";
        private string pathWithEnv;
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private string sortString = "Name";
        private string sortOrder = "ASC";
        private string statusText = "Песен: 0";
        private double progress_position = 0;
        private bool needToSort = false;
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void Init()
        {
            pathWithEnv = @"%AllUsersProfile%\myplayer";
            dirpath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            filename = "playerdb.sdf";
            if (!DBInitializer.InitDB(dirpath, filename))
            {
                MessageBox.Show("Database cannot be initialized. " +
                    "Program will be now closed", "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Close();
            }
            if (!DBInitializer.InitTables(dirpath, filename))
            {
                MessageBox.Show("Database tables cannot be initialized. " +
                    "Program will be now closed", "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Close();
            }
            FolderProcessing.Init(dirpath + "\\" + filename);
            timer1 = new DispatcherTimer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = new TimeSpan(0, 0, 1);
            timer1.Start();
            timer2 = new DispatcherTimer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        void timer2_Tick(object sender, EventArgs e)
        {
            if (listView1.SelectedIndex == -1)
            {
                label1.Content = "";
                label2.Content = "";
                label3.Content = "";
                progressBar1.Value = 0;
                progressBar1.Visibility = System.Windows.Visibility.Hidden;
                ProgressValue = 0;
            }
            else if (Player.GetLength() != 0)
            {
                int curr = Player.GetCurrPos();
                int length = Player.GetLength();
                label2.Content = "";
                label3.Content = "";
                progressBar1.Maximum = length;
                progressBar1.Visibility = System.Windows.Visibility.Visible;
                int currhour = curr / 3600;
                int currmin = (curr - (currhour * 3600)) / 60;
                int currsec = curr - (currhour * 3600) - (currmin * 60);
                int lenhour = length / 3600;
                int lenmin = (length - (lenhour * 3600)) / 60;
                int lensec = length - (lenhour * 3600) - (lenmin * 60);
                if (currhour > 0)
                {
                    label2.Content += (currhour < 10 ? "0" : "") + currhour.ToString() + ":";
                }
                label2.Content += (currmin < 10 ? "0" : "") + currmin.ToString() + ":";
                label2.Content += (currsec < 10 ? "0" : "") + currsec.ToString();
                if (lenhour > 0)
                {
                    label3.Content += (lenhour < 10 ? "0" : "") + lenhour.ToString() + ":";
                }
                label3.Content += (lenmin < 10 ? "0" : "") + lenmin.ToString() + ":";
                label3.Content += (lensec < 10 ? "0" : "") + lensec.ToString();
                progressBar1.Value = curr * progressBar1.Maximum / length;
                progress_position = progressBar1.Value;
                ProgressValue = (double)(progressBar1.Value / progressBar1.Maximum);
            }
            if (Player.IsEnded() == true)
            {
                Forward_Click(sender, new RoutedEventArgs());
            }

        }

        void timer1_Tick(object sender, EventArgs e)
        {
            music = FolderProcessing.GetObservableFromDB(dirpath + "\\" + filename, filter, sortString, sortOrder);
            if (music.Count != Music.Count || needToSort)
            {
                Music = music;
                listView1.ItemsSource = Music;
                statusText = "Песен: " + Music.Count;
                statusBarText.Text = statusText;
                needToSort = false;
            }
        }

        public MainWindow()
        {
            Init();
            Cases = new ObservableCollection<string>();
            Music = new ObservableCollection<SongDbItems>();
            music = new ObservableCollection<SongDbItems>();
            ProgressValue = 0.0;
            ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            PlayVisibility = System.Windows.Visibility.Visible;
            PauseVisibility = System.Windows.Visibility.Hidden;
            PlayEnabled = true;
            PauseEnabled = false;
            PlayPauseButtonSource = @"images\playButton.png";
            InitializeComponent();
            Cases.Add("Вся музыка");
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
                  e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;
            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }
                    string header = headerClicked.Column.Header as string;
                    switch (header)
                    {
                        case "Название":
                            sortString = "Name";
                            break;
                        case "Исполнитель":
                            sortString = "Artist";
                            break;
                        case "Альбом":
                            sortString = "Album";
                            break;
                        case "Год":
                            sortString = "Year";
                            break;
                    }
                    if (direction == ListSortDirection.Ascending)
                    {
                        sortOrder = "ASC";
                    }
                    else
                    {
                        sortOrder = "DESC";
                    }
                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }
                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                    needToSort = true;
                    timer1_Tick(sender, e);
                }
            }
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex == 0)
            {
                Cases.Remove("Результаты поиска");
                filter = "";
                textBox1.Text = "";

            }
            listBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                if (Cases.Count == 1 || Cases[1] != "Результаты поиска")
                {
                    Cases.Insert(1, "Результаты поиска");
                    listBox1.SelectedIndex = 1;
                }
                filter = textBox1.Text;
            }
            else
                listBox1.SelectedIndex = 0;
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
        }

        private void AddFolderClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();
            fd.ShowNewFolderButton = false;
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderProcessing.FoldersToAdd.Enqueue(fd.SelectedPath);
                Thread processThread = new Thread(FolderProcessing.ProcessAll);
                processThread.IsBackground = true;
                processThread.Start(dirpath + "\\" + filename);
            }
        }

        private void DeleteFolderClick(object sender, RoutedEventArgs e)
        {
            DeleteWindow dw = new DeleteWindow();
            Nullable<bool> dialogResult = dw.ShowDialog();
            if (dialogResult == true && dw.listBox1.SelectedItems.Count > 0)
            {
                FolderProcessing.DeleteFolderFromDB(Convert.ToString(dw.listBox1.SelectedItems[0]),
                    dirpath + "\\" + filename);
            }
        }

        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            if (Music.Count == 0)
            {
                return;
            }
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedIndex > 0 || ShuffleSongs)
                {
                    if (ShuffleSongs)
                    {
                        Random r = new Random();
                        listView1.SelectedIndex = r.Next(0, Music.Count);
                    }
                    else
                    {
                        listView1.SelectedIndex = listView1.SelectedIndex - 1;
                    }
                }
                else
                {
                    listView1.SelectedIndex = -1;
                    Pause_Click(sender, e);
                    return;
                }
            }
            else
            {
                if (ShuffleSongs)
                {
                    Random r = new Random();
                    listView1.SelectedIndex = r.Next(0, Music.Count);
                }
                else
                {
                    listView1.SelectedIndex = 0;
                }
            }
            listView1.Focus();
            Play_Click(sender, e);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Music.Count == 0)
            {
                return;
            }
            ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            PlayVisibility = System.Windows.Visibility.Hidden;
            PauseVisibility = System.Windows.Visibility.Visible;
            PlayEnabled = false;
            PauseEnabled = true;
            PlayPauseButtonSource = @"images\pauseButton.png";
            PlayPause.Click -= ThumbButtonInfo_PlayClick;
            PlayPause.Click += ThumbButtonInfo_PauseClick;
            if (listView1.SelectedIndex == -1)
            {
                if (ShuffleSongs)
                {
                    Random r = new Random();
                    listView1.SelectedIndex = r.Next(0, Music.Count);
                }
                else
                {
                    listView1.SelectedIndex = 0;
                }
            }
            label1.Content = Music[listView1.SelectedIndex].Name + " - " + Music[listView1.SelectedIndex].Artist;
            listView1.Focus();
            listView1.ScrollIntoView(listView1.SelectedItem);
            Player.Play(Music[listView1.SelectedIndex].Path);
            timer2.Start();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PauseVisibility = System.Windows.Visibility.Hidden;
            PlayVisibility = System.Windows.Visibility.Visible;
            PauseEnabled = false;
            PlayEnabled = true;
            PlayPauseButtonSource = @"images\playButton.png";
            PlayPause.Click += ThumbButtonInfo_PlayClick;
            PlayPause.Click -= ThumbButtonInfo_PauseClick;
            Player.Pause();
            if (listView1.SelectedIndex == -1)
            {
                Player.Stop();
            }
            timer2.Stop();
            ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (Music.Count == 0)
            {
                return;
            }
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedIndex < listView1.Items.Count - 1 || ShuffleSongs)
                {
                    if (ShuffleSongs)
                    {
                        Random r = new Random();
                        listView1.SelectedIndex = r.Next(0, Music.Count);
                    }
                    else
                    {
                        listView1.SelectedIndex = listView1.SelectedIndex + 1;
                    }
                }
                else
                {
                    listView1.SelectedIndex = -1;
                    Pause_Click(sender, e);
                    return;
                }
            }
            else
            {
                if (ShuffleSongs)
                {
                    Random r = new Random();
                    listView1.SelectedIndex = r.Next(0, Music.Count);
                }
                else
                {
                    listView1.SelectedIndex = 0;
                }
            }
            listView1.Focus();
            Play_Click(sender, e);
        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Play_Click(sender, e);
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void progressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(progressBar1.Value - progress_position) > 1)
            {
                Player.SeekPos((int)progressBar1.Value);
                progress_position = progressBar1.Value;
            }
        }

        private void ThumbButtonInfo_BackwardClick(object sender, EventArgs e)
        {
            Backward_Click(sender, new RoutedEventArgs());
        }

        private void ThumbButtonInfo_ForwardClick(object sender, EventArgs e)
        {
            Forward_Click(sender, new RoutedEventArgs());
        }

        private void ThumbButtonInfo_PlayClick(object sender, EventArgs e)
        {
            Play_Click(sender, new RoutedEventArgs());
        }

        private void ThumbButtonInfo_PauseClick(object sender, EventArgs e)
        {
            Pause_Click(sender, new RoutedEventArgs());
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            if (ShuffleSongs)
            {
                Shuffle.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0x5B, 0xBA));
            }
            else
            {
                Shuffle.Background = Brushes.Black;
            }
            ShuffleSongs = !ShuffleSongs;
        }
    }
}
