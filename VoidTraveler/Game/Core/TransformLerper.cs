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
        private readonly float SERVER_FRAME_TIME = 0.05f;

        private NetworkedEntities _entities;

        public TransformLerper(NetworkedEntities networkedEntities, World world) : base(world)
        {
            _entities = networkedEntities;
        }

        protected override void Update(LogicUpdate state, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var lerp = ref entity.Get<TransformLerp>();

            if (!lerp.CurrentTarget.HasValue)
            {
                DequeueNextTarget(transform, ref lerp);
                entity.Set(transform);
            }

            if (lerp.CurrentTarget.HasValue)
            {
                var serverFrameTime = lerp.CurrentTarget.Value.DeltaTime;
                var frameTime = (float)state.DeltaSeconds + (lerp.Messages.Count - 2) * 0.002f;
                if (lerp.Progress > serverFrameTime)
                {
                    var remainingOnTarget = serverFrameTime - lerp.Progress;
                    LerpTransform(transform, lerp.CurrentTarget.Value, remainingOnTarget, remainingOnTarget);
                    DequeueNextTarget(transform, ref lerp);
                    entity.Set(transform);
                    serverFrameTime = lerp.CurrentTarget?.DeltaTime ?? 0f;
                    frameTime = frameTime - remainingOnTarget;
                }

                if(lerp.CurrentTarget.HasValue)
                {
                    LerpTransform(transform, lerp.CurrentTarget.Value, serverFrameTime - lerp.Progress, frameTime);
                    entity.Set(transform);
                }

                lerp.Progress += frameTime;
            }

            InfoViewer.Values[$"Stack {entity.ToString()}"] = lerp.Messages.Count.ToString();

            entity.Set(lerp);
        }

        private void DequeueNextTarget(Transform transform, ref TransformLerp lerp)
        {
            if (lerp.Messages.Any())
            {
                lerp.CurrentTarget = lerp.Messages.Dequeue();
                transform.Parent = lerp.CurrentTarget.Value.ParentId.HasValue ? _entities[lerp.CurrentTarget.Value.ParentId.Value] : (Entity?)null;
            }
            else
            {
                lerp.CurrentTarget = null;
            }
            lerp.Progress = 0;
        }

        private void LerpTransform(Transform transform, TransformMessage target, float remainingTime, float deltaTime)
        {
            var multiplier = deltaTime / remainingTime;
            transform.Position += (target.Position - transform.Position) * multiplier;
            transform.Rotation += (target.Rotation - transform.Rotation) * multiplier;
            transform.Scale += (target.Scale - transform.Scale) * multiplier;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
