using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DefaultEcs;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;

namespace VoidTraveler.Game.Constructs
{
    public struct ConstructTileUserInfo
    {
        public int X;
        public int Y;
        public ConstructTile Tile;
    }

    public class ConstructBodyGenerator : PhysicsBodyGenerator<Construct>
    {
        public ConstructBodyGenerator(PhysicsSystem physicsSystem, DefaultEcs.World world) : base(physicsSystem, world)
        {
        }

        protected override void ConfigureBody(Body body, Construct source, Transform transform)
        {
            body.BodyType = BodyType.Dynamic;
            body.Position = transform.WorldPosition;

            var fixtures = body.FixtureList.ToArray();
            foreach(var fixture in fixtures)
            {
                body.DestroyFixture(fixture);
            }

            var offset = new Vector2(-source.XLength * source.TileSize / 2f, -source.YLength * source.TileSize / 2f);

            foreach (var (x, y, tile) in source.GetTiles())
            {
                if (tile.Exists && tile.Collides)
                {
                    FixtureFactory.AttachRectangle(
                        source.TileSize,
                        source.TileSize,
                        0.2f,
                        offset + new Vector2(x * source.TileSize, y * source.TileSize),
                        body,
                        new ConstructTileUserInfo() { X = x, Y = y, Tile = tile });
                }
            }
        }
    }
}
