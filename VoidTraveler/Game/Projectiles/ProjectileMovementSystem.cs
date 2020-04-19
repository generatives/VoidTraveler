using DefaultEcs;
using DefaultEcs.System;
using FarseerPhysics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;

namespace VoidTraveler.Game.Projectiles
{
    public class ProjectileMovementSystem : AEntityBufferedSystem<LogicUpdate>
    {
        private readonly PhysicsSystem _physicsSystem;

        public ProjectileMovementSystem(PhysicsSystem physicsSystem, World world) : base(world.GetEntities().With<Transform>().With<Projectile>().AsSet())
        {
            _physicsSystem = physicsSystem;
        }

        protected override void Update(LogicUpdate update, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var projectile = ref entity.Get<Projectile>();

            if(projectile.MoveDirection != Vector2.Zero)
            {
                var goalPosition = transform.Position + Vector2.Normalize(projectile.MoveDirection) * projectile.MoveSpeed * (float)update.DeltaSeconds;
                var goalWorldPosition = transform.ParentTransform.GetWorld(goalPosition);
                var playerAABB = new AABB(goalWorldPosition, projectile.Radius * 2, projectile.Radius * 2);

                var collided = false;
                _physicsSystem.World.QueryAABB(
                    (fixture) =>
                    {
                        collided = true;
                        return !collided;
                    },
                    ref playerAABB);

                if (!collided)
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
