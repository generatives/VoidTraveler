using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Networking
{
    public class EntityMessageApplier<TState, TData> : MessageReciever<TState, EntityMessage<TData>>
    {
        protected NetworkedEntities Entities { get; private set; }

        public EntityMessageApplier(NetworkedEntities entities, World world) : base(world)
        {
            Entities = entities;
        }

        protected override void Update(TState state, in EntityMessage<TData> message)
        {
            var entity = Entities[message.Id];
            Update(state, message.Data, entity);
        }

        protected virtual void Update(TState state, in TData messageData, in Entity entity) { }
    }
}
