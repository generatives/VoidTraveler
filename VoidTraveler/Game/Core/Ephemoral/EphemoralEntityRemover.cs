using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Game.Core.Ephemoral
{
    [With(typeof(EphemoralEntity))]
    public class EphemoralEntityRemover : AEntityBufferedSystem<LogicUpdate>
    {
        public EphemoralEntityRemover(World world) : base(world)
        {
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var ephemoral = ref entity.Get<EphemoralEntity>();
            ephemoral.Frames--;
            if(ephemoral.Frames == 0)
            {
                entity.Dispose();
            }
        }
    }
}
