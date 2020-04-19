using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Platform;

namespace VoidTraveler.Networking
{
    public struct ClientSystemUpdate
    {
        public List<object> Messages { get; set; }
        public IInputTracker Input { get; set; }
    }
}
