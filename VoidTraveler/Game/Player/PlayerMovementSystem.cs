using DefaultEcs;
using DefaultEcs.System;
using FarseerPhysics.Collision;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;
using VoidTraveler.Game.Projectiles;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Player
{
    public class PlayerMovementSystem : AEntitySystem<LogicUpdate>
    {
        private readonly PhysicsSystem _physicsSystem;
        private readonly World _world;

        public PlayerMovementSystem(PhysicsSystem physicsSystem, World world) : base(world.GetEntities().With<Transform>().With<Player>().AsSet())
        {
            _physicsSystem = physicsSystem;
            _world = world;
        }

        protected override void Update(LogicUpdate update, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var player = ref entity.Get<Player>();

            transform.Rotation += player.RotationAmount;

            if(player.Fire)
            {
                var projectile = _world.CreateEntity();
                var projectileTransform = new Transform() { Position = transform.Position };
                projectileTransform.SetParent(transform.Parent);
                projectile.Set(projectileTransform);
                projectile.Set(new Projectile()
                {
                    Colour = RgbaFloat.Red,
                    Radius = 2.5f,
                    MoveDirection = transform.Parent.GetLocal(player.FireAt) - transform.Parent.GetLocal(transform.WorldPosition),
                    MoveSpeed = 300f
                });
            }

            if(player.MoveDirection != Vector2.Zero)
            {
                var goalPosition = transform.Position + Vector2.Normalize(player.MoveDirection) * player.MoveSpeed * (float)update.DeltaSeconds;
                var goalWorldPosition = transform.Parent.GetWorld(goalPosition);
                var playerAABB = new AABB(goalWorldPosition, player.Radius * 2, player.Radius * 2);

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
            }
        }
    }
}
