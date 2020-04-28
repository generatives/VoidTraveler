using DefaultEcs;
using DefaultEcs.System;
using FarseerPhysics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;

namespace VoidTraveler.Game.Projectiles
{
    public class ProjectileMovementSystem : AEntityBufferedSystem<LogicUpdate>
    {
        public ProjectileMovementSystem(World world) : base(world.GetEntities().With<Transform>().With<Projectile>().AsSet())
        {
        }

        protected override void Update(LogicUpdate update, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var projectile = ref entity.Get<Projectile>();

            if(projectile.MoveDirection != Vector2.Zero)
            {
                ref var construct = ref projectile.CurrentConstruct.Get<Construct>();

                var goalPosition = transform.Position + Vector2.Normalize(projectile.MoveDirection) * projectile.MoveSpeed * (float)update.DeltaSeconds;
                if (!ConstructEntityMovement.Collides(ref construct, goalPosition, projectile.Radius * 2f))
                {
                    transform.Position = goalPosition;
                    entity.Set(transform);
                }
                else
                {
                    entity.Dispose();
                }
            }
        }
    }
}
