using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Veldrid;
using VoidTraveler.Game.Core;

namespace VoidTraveler.Game.Constructs
{
    public class ConstructRenderer : AEntitySystem<DrawDevice>
    {
        private float _graphicsScale = 20f;

        public ConstructRenderer(World world) : base(world.GetEntities().With<Transform>().With<Construct>().AsSet())
        {
        }

        protected override void Update(DrawDevice device, in Entity entity)
        {
            ref var construct = ref entity.Get<Construct>();
            ref var transform = ref entity.Get<Transform>();

            var transformMatrix = transform.GetLengthScaledWorldMatrix(Settings.GRAPHICS_SCALE);

            var offset = new Vector2(-construct.XLength, -construct.YLength) * construct.TileSize / 2f;
            var size = Vector2.One * construct.TileSize;

            foreach (var (x, y, tile) in construct.GetTiles())
            {
                if(tile.Exists)
                {
                    var position = offset + new Vector2(x, y) * construct.TileSize;
                    device.Add(device.WhitePixel,
                        size,
                        Matrix3x2.CreateTranslation(position - size / 2f) * transformMatrix,
                        tile.Colour);

                    if (tile.Collides)
                    {
                        device.Add(device.WhitePixel,
                            Vector2.One * (construct.TileSize - 0.2f),
                            Matrix3x2.CreateTranslation(new Vector2(position.X + 0.1f, position.Y + 0.1f) - size / 2f) * transformMatrix,
                            RgbaFloat.Black);
                    }
                }
            }
        }
    }
}
