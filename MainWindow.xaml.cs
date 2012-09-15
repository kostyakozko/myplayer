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

namespace myplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> cases { get; set; }

        public List<SongDbItems> music;
        
        private string dirpath;
        private string filename;
        private string pathWithEnv;
        private DispatcherTimer timer1;
        private string sortString = "Name";
        private string sortOrder = "ASC";
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
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            music = FolderProcessing.GetFilesFromDB(dirpath + "\\" + filename, "", sortString, sortOrder);
            listView1.ItemsSource = music;
        }

        public MainWindow()
        {
            Init();
            cases = new List<string>();
            music = new List<SongDbItems>();
            InitializeComponent();
            cases.Add("Вся музыка");
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
                    timer1_Tick(sender, e);
                }
            }
        }
    }
}
