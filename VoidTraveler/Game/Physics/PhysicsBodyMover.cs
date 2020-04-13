using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoidTraveler.Game.Core;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Physics
{
    public struct ControlledPhysicsBody
    {

    }

    public class PhysicsBodyMover : AEntitySystem<LogicUpdate>
    {
        public PhysicsBodyMover(World world) : base(world.GetEntities().With<ControlledPhysicsBody>().With<PhysicsBody>().AsSet())
        {
        }

        protected override void Update(LogicUpdate update, in Entity entity)
        {
            ref var constructBody = ref entity.Get<PhysicsBody>();

            var force = Vector2.Zero;
            if (update.Input.GetKey(Tortuga.Platform.TKey.A))
            {
                force += Vector2.UnitX * -1f;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.D))
            {
                force += Vector2.UnitX;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.W))
            {
                force += Vector2.UnitY;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.S))
            {
                force += Vector2.UnitY * -1;
            }

            force *= 5000;
            force = Vector2.Transform(force, Matrix3x2.CreateRotation(constructBody.Body.Rotation));
            constructBody.Body.ApplyLinearImpulse(force);

            var rotationForce = 0f;
            if (update.Input.GetKey(Tortuga.Platform.TKey.Q))
            {
                rotationForce += 1;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.E))
            {
                rotationForce += -1;
            }
            constructBody.Body.ApplyAngularImpulse(rotationForce * 300000);
            constructBody.Body.Awake = true;
        }
    }
}
