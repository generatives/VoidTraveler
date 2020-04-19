using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Game.Core
{
    public class System<T> : ISystem<T>
    {
        public bool IsEnabled { get; set; } = true;

        public virtual void Update(T state)
        {
        }

        public virtual void Dispose()
        {

        }
    }
}
