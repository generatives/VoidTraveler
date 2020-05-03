using DefaultEcs;
using DefaultEcs.System;
using Primitives2D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using VoidTraveler.Game.Core;
using VoidTraveler.Utilities;

namespace VoidTraveler.Game.Constructs.Component
{
    [With(typeof(Transform))]
    [With(typeof(Thruster))]
    public class ThrusterRenderer : AEntitySystem<DrawDevice>
    {
        public ThrusterRenderer(World world) : base(world)
        {
        }

        protected override void Update(DrawDevice state, in Entity entity)
        {
            var transform = entity.Get<Transform>();
            ref var thruster = ref entity.Get<Thruster>();

            if(thruster.Active)
            {
                var position = transform.WorldPosition;
                var dir = Vector2.UnitY.Rotate(transform.WorldRotation) * 0.5f;

                state.DrawCircle((position + 1 * dir) * Settings.GRAPHICS_SCALE, 0.5f * Settings.GRAPHICS_SCALE, 8, Veldrid.RgbaFloat.DarkRed);
                state.DrawCircle((position + 2 * dir) * Settings.GRAPHICS_SCALE, 0.3f * Settings.GRAPHICS_SCALE, 8, Veldrid.RgbaFloat.DarkRed);
                state.DrawCircle((position + 3 * dir) * Settings.GRAPHICS_SCALE, 0.1f * Settings.GRAPHICS_SCALE, 8, Veldrid.RgbaFloat.DarkRed);
            }
        }
    }
}
