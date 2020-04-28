using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoidTraveler.Editor;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Constructs
{
    [With(typeof(Construct), typeof(PhysicsBody), typeof(ConstructPilotable))]
    public class ConstructPilotSystem : AEntitySystem<LogicUpdate>
    {
        public ConstructPilotSystem(World world) : base(world)
        {
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var body = ref entity.Get<PhysicsBody>();
            ref var pilotable = ref entity.Get<ConstructPilotable>();

            var force = Vector2.Zero;
            if (pilotable.Left)
            {
                force += Vector2.UnitX * -1f;
            }
            if (pilotable.Right)
            {
                force += Vector2.UnitX;
            }
            if (pilotable.Forward)
            {
                force += Vector2.UnitY;
            }
            if (pilotable.Backward)
            {
                force += Vector2.UnitY * -1;
            }

            if(force != Vector2.Zero)
            {
                force *= 50;
                force = Vector2.Transform(force, Matrix3x2.CreateRotation(body.Body.Rotation));
                body.Body.ApplyForce(force);
            }

            var rotationForce = 0;
            if (pilotable.RotateLeft)
            {
                rotationForce += 1;
            }
            if (pilotable.RotateRight)
            {
                rotationForce += -1;
            }

            if(rotationForce != 0)
            {
                body.Body.ApplyAngularImpulse(rotationForce * 10);
                body.Body.Awake = true;
            }

            InfoViewer.Log($"V Force {entity.ToString()}", $"X: {force.X}, Y: {force.Y}");
            InfoViewer.Log($"Vel {entity.ToString()}", $"X: {body.Body.LinearVelocity.X}, Y: {body.Body.LinearVelocity.Y}");
        }
    }
}
