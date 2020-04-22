using DefaultEcs.System;
using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoidTraveler.Game.Physics
{
    public class PhysicsSystem : ISystem<LogicUpdate>
    {
        public World World { get; private set; }

        public bool IsEnabled { get; set; } = true;

        public PhysicsSystem(World world)
        {
            World = world;
        }

        public void Update(LogicUpdate state)
        {
            World.Step((float)state.DeltaSeconds);
        }

        public void Dispose()
        {

        }
    }
}
