using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace VoidTraveler.Game.Constructs
{
    [MessagePackObject]
    public struct ConstructTile
    {
        [Key(0)]
        public RgbaFloat Colour { get; set; }
        [Key(1)]
        public bool Collides { get; set; }
        [Key(2)]
        public bool Exists { get; set; }
    }
}
