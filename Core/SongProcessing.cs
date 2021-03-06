using System;
using System.IO;
using Shell32;

namespace MyPlayer
{
    public struct SongDbItems
    {
        public int id;
        public string name;
        public string artist;
        public string album;
        public int year;
        public string path;
        public string rootdir;
        
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Rootdir
        {
            get { return rootdir; }
            set { rootdir = value; }
        }
        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }
        public string Album
        {
            get { return album; }
            set { album = value; }
        }
        public int Year 
        {
            get { return year; }
            set { year = value; }
        }
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public SongDbItems (int id, string name, string rootdir, string artist, string album,
                int year, string path)
        {
            this.id = id;
            this.name = name;
            this.rootdir = rootdir;
            this.artist = artist;
            this.album = album;
            this.year = year;
            this.path = path;
        }
    };
    public static class SongProcessing
    {

        private static SongDbItems GetSongInfo(string filepath)
        {
            SongDbItems songInfo = new SongDbItems();
            songInfo.path = filepath;
            Shell shell = new Shell();
            string filedir = Path.GetDirectoryName(filepath);
            string filename = Path.GetFileName(filepath);
            Folder objFolder = shell.NameSpace(filedir);
            if (objFolder != null)
            {
                FolderItem fi = objFolder.ParseName(filename);
                songInfo.name = objFolder.GetDetailsOf(fi, 21);
                songInfo.album = objFolder.GetDetailsOf(fi, 14);
                songInfo.artist = objFolder.GetDetailsOf(fi, 20);
                string year_string = objFolder.GetDetailsOf(fi, 15);
                if (!String.IsNullOrEmpty(year_string))
                {
                    songInfo.year = int.Parse(year_string);
                }
                else
                {
                    songInfo.year = 0;
                }
                if (songInfo.name.Contains("'"))
                {
                    songInfo.name = songInfo.name.Replace("'", "`");
                }
                if (songInfo.album.Contains("'"))
                {
                    songInfo.album = songInfo.album.Replace("'", "`");
                }
                if (songInfo.artist.Contains("'"))
                {
                    songInfo.artist = songInfo.artist.Replace("'", "`");
                }
                if (songInfo.path.Contains("'"))
                {
                    songInfo.path = songInfo.path.Replace("'", "`");
                }
            }

            return songInfo;
        }

        public static bool ProcessSong(string filepath, string rootdir)
        {
            try
            {
                SongDbItems song = GetSongInfo(filepath);
                SongQueue.queueMutex.WaitOne();
                song.rootdir = rootdir;
                if (song.rootdir.Contains("'"))
                {
                    song.rootdir = song.rootdir.Replace("'", "`");
                }
                SongQueue.items.Enqueue(song);
                SongQueue.queueMutex.ReleaseMutex();
            }
            catch
            {
                try
                {
                    SongQueue.queueMutex.ReleaseMutex();
                }
                catch
                {
                }
                return false;
            }
            return true;
        }

        public static bool SetSongParams(string filepath, SongDbItems parameters)
        {
            try
            {
                SongDbItems song = GetSongInfo(filepath);
                if (String.IsNullOrEmpty(song.path))
                {
                    return false;
                }
                song.name = parameters.name.Replace("'", ".");
                song.album = parameters.album.Replace("'", ".");
                song.artist = parameters.artist.Replace("'", ".");
                song.year = parameters.year;
                song.rootdir = parameters.rootdir;
                SongQueue.queueMutex.WaitOne();
                SongQueue.items.Enqueue(song);
                SongQueue.queueMutex.ReleaseMutex();
            }
            catch
            {
                try
                {
                    SongQueue.queueMutex.ReleaseMutex();
                }
                catch
                {
                }
                return false;
            }
            return true;
        }
    }
}
