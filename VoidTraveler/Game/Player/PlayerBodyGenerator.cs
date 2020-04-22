using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DefaultEcs;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using VoidTraveler.Core;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Player
{
    public class PlayerBodyGenerator : PhysicsBodyGenerator<Player>
    {
        public PlayerBodyGenerator(FarseerPhysics.Dynamics.World physicsWorld, DefaultEcs.World world) : base(physicsWorld, world)
        {
        }

        protected override void ConfigureBody(Body body, Player player, Transform transform)
        {
            body.BodyType = BodyType.Dynamic;
            body.Position = transform.WorldPosition;
            body.FixedRotation = true;

            FixtureFactory.AttachCircle(player.Radius, 1, body);
        }
    }
}
