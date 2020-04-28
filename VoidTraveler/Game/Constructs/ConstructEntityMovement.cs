using FarseerPhysics.Collision;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoidTraveler.Game.Constructs
{
    public static class ConstructEntityMovement
    {
        public static bool Collides(ref Construct construct, Vector2 goalPosition, float size)
        {
            var playerAABB = new AABB(goalPosition, size, size);
            var (goalXIndex, goalYIndex) = construct.GetIndex(goalPosition);

            var offset = new Vector2(-construct.HalfWidth, -construct.HalfHeight) + (Vector2.One * construct.TileSize / 2f);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var xIndex = goalXIndex + x;
                    var yIndex = goalYIndex + y;
                    if (construct.ContainsCollider(xIndex, yIndex))
                    {
                        var tileAABB = new AABB((new Vector2(xIndex, yIndex) * construct.TileSize) + offset, construct.TileSize, construct.TileSize);

                        if (AABB.TestOverlap(ref tileAABB, ref playerAABB)) return true;
                    }
                }
            }

            return false;
        }
    }
}
