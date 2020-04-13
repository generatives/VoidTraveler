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
    class ConstructRenderer : AEntitySystem<DrawDevice>
    {
        public ConstructRenderer(World world) : base(world.GetEntities().With<Transform>().With<Construct>().AsSet())
        {
        }

        protected override void Update(DrawDevice device, in Entity entity)
        {
            ref var construct = ref entity.Get<Construct>();
            ref var transform = ref entity.Get<Transform>();

            var transformMatrix = transform.Matrix;

            foreach (var component in construct.Components)
            {
                device.Add(device.WhitePixel,
                    component.Size,
                    Matrix3x2.CreateTranslation(component.Position - component.Size / 2f) * transformMatrix,
                    component.Colour);

                if (component.Collides)
                {
                    device.Add(device.WhitePixel,
                        new Vector2(component.Size.X - 4f, component.Size.Y - 4f),
                        Matrix3x2.CreateTranslation(new Vector2(component.Position.X + 2f, component.Position.Y + 2f) - component.Size / 2f) * transformMatrix,
                        RgbaFloat.Black);
                }
            }
        }
    }
}
