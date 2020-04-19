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
using VoidTraveler.Networking;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Player
{
    [With(typeof(Transform), typeof(Player))]
    public class PlayerMovementSystem : AEntitySystem<LogicUpdate>
    {
        private readonly PhysicsSystem _physicsSystem;
        private readonly World _world;

        public PlayerMovementSystem(PhysicsSystem physicsSystem, World world) : base(world)
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
                projectile.Set(new NetworkedEntity() { Id = Guid.NewGuid() });
                projectile.Set(new Transform() { Position = transform.Position, Parent = transform.Parent });
                projectile.Set(new Projectile()
                {
                    Colour = RgbaFloat.Red,
                    Radius = 0.25f,
                    MoveDirection = transform.ParentTransform.GetLocal(player.FireAt) - transform.ParentTransform.GetLocal(transform.WorldPosition),
                    MoveSpeed = 10f
                });
                player.Fire = false;
            }

            if(player.MoveDirection != Vector2.Zero)
            {
                var goalPosition = transform.Position + Vector2.Normalize(player.MoveDirection) * player.MoveSpeed * (float)update.DeltaSeconds;
                var goalWorldPosition = transform.ParentTransform.GetWorld(goalPosition);
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
