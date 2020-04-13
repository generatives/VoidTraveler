using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace VoidTraveler.Game.Constructs.Components
{
    public class ConstructComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public RgbaFloat Colour { get; set; }
        public bool Collides { get; set; }
    }
}
