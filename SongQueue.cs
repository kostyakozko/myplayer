using System.Collections.Generic;
using System.Threading;

namespace MyPlayer
{
    public static class SongQueue
    {
        public static Queue<SongDbItems> items = new Queue<SongDbItems>();
        public static Mutex queueMutex = new Mutex();
    }
}
