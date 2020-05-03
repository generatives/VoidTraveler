using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Constructs.Component
{
    [MessagePackObject]
    public struct ThrusterMessage
    {
        [Key(0)]
        public bool Active;
        [Key(1)]
        public float Thrust;
    }

    [With(typeof(Thruster), typeof(NetworkedEntity))]
    public class ThrusterServerSystem : AEntitySystem<ServerSystemUpdate>
    {
        public ThrusterServerSystem(World world) : base(world)
        {
        }

        protected override void Update(ServerSystemUpdate state, in Entity entity)
        {
            var thruster = entity.Get<Thruster>();
            ref var netEntity = ref entity.Get<NetworkedEntity>();

            state.Messages.Add(new EntityMessage<ThrusterMessage>(netEntity.Id, new ThrusterMessage()
            {
                Active = thruster.Active,
                Thrust = thruster.Thrust
            }));
        }
    }

    public class ThrusterMessageApplier : EntityMessageApplier<ThrusterMessage>
    {
        public ThrusterMessageApplier(NetworkedEntities entities) : base(entities) { }

        protected override void On(in ThrusterMessage messageData, in Entity entity)
        {
            var thruster = new Thruster()
            {
                Active = messageData.Active,
                Thrust = messageData.Thrust
            };
            entity.Set(thruster);
        }
    }
}
