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

namespace myplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> cases { get; set; }

        public class SongDbItems
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Rootdir { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string Year { get; set; }
            public string Path { get; set; }
            public SongDbItems (string id, string name, string rootdir, string artist, string album,
                string year, string path )
            {
                Id = id;
                Name = name;
                Rootdir = rootdir;
                Artist = artist;
                Album = album;
                Year = year;
                Path = path;
            }
        };

        public List<SongDbItems> music;
        public List<string> music_id { get; set; }
       
        public MainWindow()
        {
            cases = new List<string>();
            music = new List<SongDbItems>();
            music_id = new List<string>();
            int i=1;
            InitializeComponent();
            cases.Add("Вся музыка");
            music.Add(new SongDbItems("1", "2", "3", "4", "5", "6", "7"));


            listView1.ItemsSource = music;
        }
    }
}
