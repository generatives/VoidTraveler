using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Core;

namespace VoidTraveler.Game.Projectiles
{
    public class ProjectileRenderer : AEntitySystem<DrawDevice>
    {
        public ProjectileRenderer(World world) : base(world.GetEntities().With<Transform>().With<Projectile>().AsSet())
        {
        }

        protected override void Update(DrawDevice device, in Entity entity)
        {
            ref var projectile = ref entity.Get<Projectile>();
            ref var transform = ref entity.Get<Transform>();

            Primitives2D.SpriteBatchExtensions.DrawCircle(device, transform.WorldPosition, projectile.Radius, 8, projectile.Colour);
        }
    }
}
