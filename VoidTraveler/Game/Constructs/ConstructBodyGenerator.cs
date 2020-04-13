using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DefaultEcs;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;

namespace VoidTraveler.Game.Constructs
{
    public class ConstructBodyGenerator : PhysicsBodyGenerator<Construct>
    {
        public ConstructBodyGenerator(PhysicsSystem physicsSystem, DefaultEcs.World world) : base(physicsSystem, world)
        {
        }

        protected override void ConfigureBody(Body body, Construct source, Transform transform)
        {
            body.BodyType = BodyType.Dynamic;
            body.Position = transform.WorldPosition;

            var fixtures = body.FixtureList.ToArray();
            foreach(var fixture in fixtures)
            {
                body.DestroyFixture(fixture);
            }

            foreach (var component in source.Components)
            {
                if (component.Collides)
                {
                    FixtureFactory.AttachRectangle(
                        component.Size.X,
                        component.Size.Y,
                        0.2f,
                        new Vector2(component.Position.X, component.Position.Y),
                        body,
                        component);
                }
            }
        }
    }
}
