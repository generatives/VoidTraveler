using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Platform;

namespace VoidTraveler
{
    public struct LogicUpdate
    {
        public double DeltaSeconds { get; private set; }
        public IInputTracker Input { get; private set; }

        public LogicUpdate(double deltaSeconds, IInputTracker input)
        {
            DeltaSeconds = deltaSeconds;
            Input = input;
        }
    }
}
