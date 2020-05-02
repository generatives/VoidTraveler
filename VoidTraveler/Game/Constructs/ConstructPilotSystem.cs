using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoidTraveler.Editor;
using VoidTraveler.Game.Constructs.Component;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Constructs
{
    [With(typeof(Construct), typeof(PhysicsBody), typeof(ConstructPilotingControl))]
    public class ConstructPilotSystem : AEntitySystem<LogicUpdate>
    {
        public ConstructPilotSystem(World world) : base(world)
        {
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var control = ref entity.Get<ConstructPilotingControl>();

            if(control.Cheat)
            {
                CheatMove(ref entity.Get<PhysicsBody>(), ref control);
            }
            else
            {
                Move(ref entity.Get<ConstructPilotable>(), ref control);
            }
        }

        private void Move(ref ConstructPilotable pilotable, ref ConstructPilotingControl control)
        {
            pilotable.All.ForEach(e => SetThruster(ref e, false));
            if (control.Forward)
            {
                pilotable.PosY.ForEach(e => SetThruster(ref e, true));
            }
            if (control.Backward)
            {
                pilotable.NegY.ForEach(e => SetThruster(ref e, true));
            }

            if (control.Left)
            {
                pilotable.NegX.ForEach(e => SetThruster(ref e, true));
            }
            if (control.Right)
            {
                pilotable.PosX.ForEach(e => SetThruster(ref e, true));
            }

            if (control.RotateLeft)
            {
                pilotable.PosRot.ForEach(e => SetThruster(ref e, true));
            }
            if (control.RotateRight)
            {
                pilotable.NegRot.ForEach(e => SetThruster(ref e, true));
            }
        }

        private void SetThruster(ref Entity e, bool set)
        {
            ref var thruster = ref e.Get<Thruster>();
            thruster.Active = set;
            e.Set(thruster);
        }

        private void CheatMove(ref PhysicsBody body, ref ConstructPilotingControl control)
        {

            var force = Vector2.Zero;
            if (control.Left)
            {
                force += Vector2.UnitX * -1f;
            }
            if (control.Right)
            {
                force += Vector2.UnitX;
            }
            if (control.Forward)
            {
                force += Vector2.UnitY;
            }
            if (control.Backward)
            {
                force += Vector2.UnitY * -1;
            }

            if (force != Vector2.Zero)
            {
                force *= 50;
                force = Vector2.Transform(force, Matrix3x2.CreateRotation(body.Body.Rotation));
                body.Body.ApplyForce(force);
            }

            var rotationForce = 0;
            if (control.RotateLeft)
            {
                rotationForce += 1;
            }
            if (control.RotateRight)
            {
                rotationForce += -1;
            }

            if (rotationForce != 0)
            {
                body.Body.ApplyAngularImpulse(rotationForce * 10);
                body.Body.Awake = true;
            }
        }
    }
}
