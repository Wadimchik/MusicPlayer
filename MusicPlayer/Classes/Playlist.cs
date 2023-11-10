using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
    [Serializable]
    public class Playlist
    {
        public List<Track> Tracks { get; set; } = new List<Track>();
    }
}
