using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Shell32;
using Microsoft.DirectX.AudioVideoPlayback;

namespace myplayer
{
    public struct SongDbItems
    {
        public string name;
        public string artist;
        public string album;
        public int year;
        public string path;
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
                songInfo.year = int.Parse(objFolder.GetDetailsOf(fi, 15));
            }
    
            return songInfo;
        }

        public static bool ProcessSong(string filepath)
        {
            try
            {
                Audio a = new Audio(filepath);
                SongQueue.queueMutex.WaitOne();
                SongQueue.items.Enqueue(GetSongInfo(filepath));
                SongQueue.queueMutex.ReleaseMutex();
            }
            catch
            {
                SongQueue.queueMutex.ReleaseMutex();
                return false;
            }
            return true;
        }

        public static bool SetSongName(string filepath, string songname)
        {
            return true;
        }

        public static bool SetSongAlbum(string filepath, string albumname)
        {
            return true;
        }

        public static bool SetSongYear(string filepath, int year)
        {
            return true;
        }

        public static bool SetSongArtist(string filepath, string artistname)
        {
            return true;
        }
    }
}
