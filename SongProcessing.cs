using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Shell32;
//using Microsoft.DirectX.AudioVideoPlayback;
using System.Threading;

namespace myplayer
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
                if (Path.GetExtension(filepath) != ".mp3" &&
                    Path.GetExtension(filepath) != ".wav")
                {
                    return false;
                }
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

        public static bool SetSongName(string filepath, string songname)
        {
            try
            {
                SongDbItems song = GetSongInfo(filepath);
                if (String.IsNullOrEmpty(song.path))
                {
                    return false;
                }
                song.name = songname.Replace("'", ".");
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

        public static bool SetSongAlbum(string filepath, string albumname)
        {
            try
            {
                SongDbItems song = GetSongInfo(filepath);
                if (String.IsNullOrEmpty(song.path))
                {
                    return false;
                }
                song.album = albumname.Replace("'", ".");
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

        public static bool SetSongYear(string filepath, int year)
        {
            try
            {
                SongDbItems song = GetSongInfo(filepath);
                if (String.IsNullOrEmpty(song.path))
                {
                    return false;
                }
                song.year = year;
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

        public static bool SetSongArtist(string filepath, string artistname)
        {
            try
            {
                SongDbItems song = GetSongInfo(filepath);
                if (String.IsNullOrEmpty(song.path))
                {
                    return false;
                }
                song.artist = artistname.Replace("'", ".");
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
