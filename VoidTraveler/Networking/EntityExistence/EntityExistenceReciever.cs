using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Networking.EntityExistence
{
    public class EntityAdder : MessageReciever<LogicUpdate, EntityMessage<EntityAdded>>
    {
        private World _world;

        public EntityAdder(World world) : base(world)
        {
            _world = world;
        }

        protected override void Update(LogicUpdate state, in EntityMessage<EntityAdded> message)
        {
            var newEntity = _world.CreateEntity();
            newEntity.Set(new NetworkedEntity() { Id = message.Id });
        }
    }
    public class EntityRemover : EntityMessageApplier<LogicUpdate, EntityRemoved>
    {
        public EntityRemover(World world) : base(world)
        {
        }

        protected override void Update(LogicUpdate state, in EntityRemoved messageData, in Entity entity)
        {
            entity.Dispose();
        }
    }
}
