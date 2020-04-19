using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Networking
{
    public class MessageReciever<TState, TMessage> : AEntitySystem<TState>
    {
        public MessageReciever(World world) : base(world.GetEntities().With<TMessage>().AsSet())
        {

        }

        protected override void Update(TState state, in Entity entity)
        {
            ref var message = ref entity.Get<TMessage>();

            Update(state, message);
        }

        protected virtual void Update(TState state, in TMessage message) { }
    }
}
