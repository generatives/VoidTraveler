using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace VoidTraveler.Game.Projectiles
{
    public struct Projectile
    {
        public float Radius { get; set; }
        public RgbaFloat Colour { get; set; }
        public Vector2 MoveDirection { get; set; }
        public float MoveSpeed { get; set; }
    }
}
