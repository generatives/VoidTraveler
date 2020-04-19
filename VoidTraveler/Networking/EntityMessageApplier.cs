using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Networking
{
    public class EntityMessageApplier<TState, TData> : MessageReciever<TState, EntityMessage<TData>>
    {
        protected EntityMap<NetworkedEntity> Entities { get; private set; }

        public EntityMessageApplier(World world) : base(world)
        {
            Entities = world.GetEntities().With<NetworkedEntity>().AsMap<NetworkedEntity>();
        }

        protected override void Update(TState state, in EntityMessage<TData> message)
        {
            var entity = Entities[new NetworkedEntity() { Id = message.Id }];
            Update(state, message.Data, entity);
        }

        protected virtual void Update(TState state, in TData messageData, in Entity entity) { }
    }
}
