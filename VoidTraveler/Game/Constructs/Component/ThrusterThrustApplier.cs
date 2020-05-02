using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoidTraveler.Game.Core;
using VoidTraveler.Physics;
using VoidTraveler.Utilities;

namespace VoidTraveler.Game.Constructs.Component
{
    [With(typeof(Transform))]
    [With(typeof(ConstructComponent))]
    [With(typeof(Thruster))]
    public class ThrusterThrustApplier : AEntitySystem<LogicUpdate>
    {
        public ThrusterThrustApplier(World world) : base(world)
        {
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var thruster = ref entity.Get<Thruster>();

            if(thruster.Active)
            {
                var transform = entity.Get<Transform>();
                ref var component = ref entity.Get<ConstructComponent>();

                var constructBody = component.Construct.Get<PhysicsBody>();

                var thrustPos = transform.WorldPosition;
                var thrustForce = Vector2.UnitY.Rotate(transform.WorldRotation + MathF.PI) * thruster.Thrust;

                constructBody.Body.ApplyForce(thrustForce, thrustPos);
            }
        }
    }
}
