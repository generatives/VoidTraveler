using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Networking
{
    public struct NetworkedEntity
    {
        public Guid Id { get; set; }
        public List<object> Messages { get; set; }
    }
}
