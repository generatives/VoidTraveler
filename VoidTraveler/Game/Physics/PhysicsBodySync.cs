using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoidTraveler.Game.Core;

namespace VoidTraveler.Physics
{
    public class PhysicsBodySync : AEntitySystem<LogicUpdate>
    {
        public PhysicsBodySync(World world) : base(world.GetEntities().With<Transform>().With<PhysicsBody>().AsSet())
        {
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var constructBody = ref entity.Get<PhysicsBody>();

            transform.WorldPosition = constructBody.Body.Position;
            transform.WorldRotation = constructBody.Body.Rotation;
        }
    }
}
