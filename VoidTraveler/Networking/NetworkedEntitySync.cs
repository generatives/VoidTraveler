using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Networking
{
    public class NetworkedEntitySync : AEntitySystem<NetworkUpdate>
    {
        public NetworkedEntitySync(World world) : base(world.GetEntities().WhenAdded<NetworkedEntity>().AsSet())
        {
        }
    }
}
