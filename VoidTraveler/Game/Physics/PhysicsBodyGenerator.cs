using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Text;
using VoidTraveler.Core;
using VoidTraveler.Game.Core;
using VoidTraveler.Physics;

namespace VoidTraveler.Game.Physics
{
    public class PhysicsBodyGenerator<TSource> : ComponentChangeSystem<LogicUpdate>
    {
        private readonly PhysicsSystem _physicsSystem;

        public PhysicsBodyGenerator(PhysicsSystem physicsSystem, DefaultEcs.World world) : base(world, typeof(TSource), typeof(Transform), typeof(PhysicsBody))
        {
            _physicsSystem = physicsSystem;
        }

        protected override void Compute(LogicUpdate state, in DefaultEcs.Entity e)
        {
            ref var transform = ref e.Get<Transform>();
            ref var source = ref e.Get<TSource>();
            ref var physicsBody = ref e.Get<PhysicsBody>();

            if(physicsBody.Body == null)
            {
                physicsBody.Body = new Body(_physicsSystem.World, e);
            }

            ConfigureBody(physicsBody.Body, source, transform);
        }

        protected virtual void ConfigureBody(Body body, TSource source, Transform transform) { }

        protected override void Remove(in DefaultEcs.Entity e)
        {
            ref var physicsBody = ref e.Get<PhysicsBody>();

            if(physicsBody.Body != null)
            {
                _physicsSystem.World.RemoveBody(physicsBody.Body);
            }
        }
    }
}
