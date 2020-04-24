using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Projectiles
{
    [MessagePackObject]
    public struct ProjectileMessage
    {
        [Key(0)]
        public float Radius { get; set; }
        [Key(1)]
        public RgbaFloat Colour { get; set; }
        [Key(2)]
        public Vector2 MoveDirection { get; set; }
        [Key(3)]
        public float MoveSpeed { get; set; }
    }

    [With(typeof(Projectile), typeof(NetworkedEntity))]
    public class ProjectileServerSystem : AEntitySystem<ServerSystemUpdate>
    {
        public ProjectileServerSystem(World world) : base(world)
        {
        }

        protected override void Update(ServerSystemUpdate state, in Entity entity)
        {
            var projectile = entity.Get<Projectile>();
            ref var netEntity = ref entity.Get<NetworkedEntity>();

            state.Messages.Add(new EntityMessage<ProjectileMessage>(netEntity.Id, new ProjectileMessage()
            {
                Radius = projectile.Radius,
                Colour = projectile.Colour,
                MoveDirection = projectile.MoveDirection,
                MoveSpeed = projectile.MoveSpeed
            }));
        }
    }

    public class ProjectileMessageApplier : EntityMessageApplier<ProjectileMessage>
    {
        public ProjectileMessageApplier(NetworkedEntities entities) : base(entities) { }

        protected override void On(in ProjectileMessage messageData, in Entity entity)
        {
            var projectile = new Projectile()
            {
                Radius = messageData.Radius,
                Colour = messageData.Colour,
                MoveDirection = messageData.MoveDirection,
                MoveSpeed = messageData.MoveSpeed
            };
            entity.Set(projectile);
        }
    }
}
