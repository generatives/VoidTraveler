using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace VoidTraveler.Game.Constructs
{
    public enum ConstructTileOrientation
    {
        NORTH = 0,
        WEST = 1,
        SOUTH = 2,
        EAST = 3
    }

    [MessagePackObject]
    public struct ConstructTile
    {
        [Key(0)]
        public bool Exists { get; set; }
        [Key(1)]
        public bool Collides { get; set; }
        [Key(2)]
        public ConstructTileOrientation Orientation; 
        [Key(3)]
        public RgbaFloat Colour { get; set; }
    }
}
