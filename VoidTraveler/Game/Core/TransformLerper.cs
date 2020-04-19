using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoidTraveler.Editor;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Core
{
    [With(typeof(Transform))]
    [With(typeof(TransformLerp))]
    public class TransformLerper : AEntitySystem<LogicUpdate>
    {
        private readonly float SERVER_FRAME_TIME = 0.045f;

        private EntityMap<NetworkedEntity> _entities;

        public TransformLerper(World world) : base(world)
        {
            _entities = world.GetEntities().With<NetworkedEntity>().AsMap<NetworkedEntity>();
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var lerp = ref entity.Get<TransformLerp>();

            if (!lerp.CurrentTarget.HasValue || lerp.Progress >= SERVER_FRAME_TIME)
            {
                if(lerp.Messages.Any())
                {
                    lerp.CurrentTarget = lerp.Messages.Dequeue();
                    transform.Parent = lerp.CurrentTarget.Value.ParentId.HasValue ? _entities[new NetworkedEntity() { Id = lerp.CurrentTarget.Value.ParentId.Value }] : (Entity?)null;
                }
                else
                {
                    lerp.CurrentTarget = null;
                }
                lerp.Progress = 0;
            }

            if(lerp.CurrentTarget.HasValue)
            {
                var target = lerp.CurrentTarget.Value;
                transform.Position += (target.Position - transform.Position) * ((float)state.DeltaSeconds / SERVER_FRAME_TIME);
                transform.Rotation += (target.Rotation - transform.Rotation) * ((float)state.DeltaSeconds / SERVER_FRAME_TIME);
                transform.Scale += (target.Scale - transform.Scale) * ((float)state.DeltaSeconds / SERVER_FRAME_TIME);
                lerp.Progress += (float)state.DeltaSeconds;

                entity.Set(transform);
            }

            InfoViewer.Values[$"Stack {entity.ToString()}"] = lerp.Messages.Count.ToString();

            entity.Set(lerp);
        }

        public override void Dispose()
        {
            base.Dispose();
            _entities.Dispose();
        }
    }
}
