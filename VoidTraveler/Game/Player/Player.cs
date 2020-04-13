using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace VoidTraveler.Game.Player
{
    public struct Player
    {
        public float Radius { get; set; }
        public RgbaFloat Colour { get; set; }
        public float MoveSpeed { get; set; }
        public Vector2 MoveDirection { get; set; }
        public float RotationAmount { get; set; }
        public bool Fire { get; set; }
        public Vector2 FireAt { get; set; }
    }
}
