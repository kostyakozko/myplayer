using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MyPlayer
{
    public static class Player
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string lpstrCommand, StringBuilder lpstrReturnString,
        int uReturnLength, IntPtr hwndCallback);
        private static string nowPlaying = "";
        public static void Play(string path)
        {
            string commandString = "";
            if (String.IsNullOrEmpty(nowPlaying) ||
                   (path != nowPlaying))
            {
                if (!String.IsNullOrEmpty(nowPlaying))
                {
                    Stop();
                }
            }
            nowPlaying = path;

            switch (System.IO.Path.GetExtension(nowPlaying))
            {
                case ".mp3":
                    commandString = "open " + "\"" + nowPlaying + "\"" +
                    " type MPEGVideo alias Mp3File";
                    break;
                case ".wav":
                    commandString = "open " + "\"" + nowPlaying + "\"" +
                    " type waveaudio alias WavFile";
                    break;
            }
            mciSendString(commandString, null, 0, IntPtr.Zero);
            switch (System.IO.Path.GetExtension(nowPlaying))
            {
                case ".mp3":
                    commandString = "play Mp3File";
                    break;
                case ".wav":
                    commandString = "play WavFile";
                    break;
            }
            mciSendString(commandString, null, 0, IntPtr.Zero);
        }
        public static void Pause()
        {
            string commandString = "";
            if (String.IsNullOrEmpty(nowPlaying))
            {
                return;
            }
            switch (System.IO.Path.GetExtension(nowPlaying))
            {
                case ".mp3":
                    commandString = "pause Mp3File";
                    break;
                case ".wav":
                    commandString = "pause WavFile";
                    break;
            }
            mciSendString(commandString, null, 0, IntPtr.Zero);
        }

        public static void Stop()
        {
            string commandString = "";
            if (String.IsNullOrEmpty(nowPlaying))
            {
                return;
            }
            switch (System.IO.Path.GetExtension(nowPlaying))
            {
                case ".mp3":
                    commandString = "close Mp3File";
                    break;
                case ".wav":
                    commandString = "close WavFile";
                    break;
            }
            mciSendString(commandString, null, 0, IntPtr.Zero);
            nowPlaying = "";
        }

        new private static string GetType()
        {
            string str = "";
            switch (System.IO.Path.GetExtension(nowPlaying))
            {
                case ".mp3":
                    str = "Mp3File";
                    break;
                case ".wav":
                    str = "WavFile";
                    break;
            }
            return str;
        }

        public static int GetLength()
        {
            string commandString = "";
            string str = GetType();
            StringBuilder buffer = new StringBuilder(128);
            int length = 0;
            commandString = "Status " + str + " length";
            mciSendString(commandString, buffer, 128, IntPtr.Zero);
            if (!String.IsNullOrEmpty(buffer.ToString()))
            {
                length = int.Parse(buffer.ToString()) / 1000;
            }
            return length;
        }

        public static void SeekMillis(int millis)
        {
            string commandString = "";
            string str = GetType();
            commandString = "seek " + str + " to " + (millis).ToString();
            mciSendString(commandString, null, 0, IntPtr.Zero);
            commandString = "play " + str;
            mciSendString(commandString, null, 0, IntPtr.Zero);
        }

        public static void SeekPos(int seconds, int millis = 0)
        {
            SeekMillis(seconds * 1000 + millis);
        }

        public static void SeekPos(int minutes, int seconds, int millis = 0)
        {
            SeekMillis(minutes * 60000 + seconds * 1000+ millis);
        }

        public static void SeekPos(int hours, int minutes, int seconds, int millis = 0)
        {
            SeekMillis(hours * 3600000 + minutes * 60000 + seconds * 1000 + millis);
        }

        public static int GetCurrPos()
        {
            string commandString = "";
            string str = GetType();
            StringBuilder buffer = new StringBuilder(128);
            int currPos = 0;
            commandString = "Status " + str + " position";
            mciSendString(commandString, buffer, 128, IntPtr.Zero);
            if (!String.IsNullOrEmpty(buffer.ToString()))
            {
                currPos = int.Parse(buffer.ToString()) / 1000;
            }
            return currPos;
        }
        public static bool IsEnded()
        {           
            if (GetCurrPos() == GetLength() && GetLength() != 0)
            {
                return true;
            }
            return false;
        }
    }
}
