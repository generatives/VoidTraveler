using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Core;

namespace VoidTraveler.Game.Player
{
    public class PlayerRenderer : AEntitySystem<DrawDevice>
    {
        public PlayerRenderer(World world) : base(world.GetEntities().With<Transform>().With<Player>().AsSet())
        {
        }

        protected override void Update(DrawDevice device, in Entity entity)
        {
            ref var player = ref entity.Get<Player>();
            ref var transform = ref entity.Get<Transform>();

            Primitives2D.SpriteBatchExtensions.DrawCircle(device, transform.WorldPosition, player.Radius, 8, player.Colour);
        }
    }
}
