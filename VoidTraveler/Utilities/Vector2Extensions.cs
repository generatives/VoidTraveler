using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoidTraveler.Utilities
{
    public static class Vector2Extensions
    {
        public static Vector2 Rotate(this Vector2 v, float rad)
        {
            float sin = MathF.Sin(rad);
            float cos = MathF.Cos(rad);

            float tx = v.X;
            float ty = v.Y;
            v.X = (cos * tx) - (sin * ty);
            v.Y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}
