using DefaultEcs;
using DefaultEcs.System;
using FarseerPhysics.Collision;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using VoidTraveler.Game.Constructs;
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
        private readonly World _world;

        public PlayerMovementSystem(World world) : base(world)
        {
            _world = world;
        }

        protected override void Update(LogicUpdate update, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var player = ref entity.Get<Player>();

            transform.Rotation += player.RotationAmount;

            if (player.Fire)
            {
                var projectile = _world.CreateEntity();
                projectile.Set(new NetworkedEntity() { Id = Guid.NewGuid() });
                projectile.Set(new Transform() { Position = transform.Position, Parent = transform.Parent });
                projectile.Set(new Projectile()
                {
                    Colour = RgbaFloat.Red,
                    Radius = 0.25f,
                    MoveDirection = transform.ParentTransform.GetLocal(player.FireAt) - transform.ParentTransform.GetLocal(transform.WorldPosition),
                    MoveSpeed = 10f,
                    CurrentConstruct = player.CurrentConstruct
                });
                player.Fire = false;
            }

            if(player.MoveDirection != Vector2.Zero)
            {
                ref var construct = ref player.CurrentConstruct.Get<Construct>();

                var goalPosition = transform.Position + Vector2.Normalize(player.MoveDirection) * player.MoveSpeed * (float)update.DeltaSeconds;
                if(!ConstructEntityMovement.Collides(ref construct, goalPosition, player.Radius * 2f))
                {
                    transform.Position = goalPosition;
                    entity.Set(transform);
                }
            }
        }
    }
}
