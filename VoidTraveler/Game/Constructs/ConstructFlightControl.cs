using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;
using VoidTraveler;
using VoidTraveler.Core;
using VoidTraveler.Game.Constructs.Component;
using VoidTraveler.Game.Core;

namespace VoidTraveler.Game.Constructs
{
    public class ConstructFlightControl : ComponentChangeSystem<LogicUpdate>
    {
        public ConstructFlightControl(World world) : base(world, typeof(Construct), typeof(ConstructPilotable))
        {
        }

        protected override void Compute(LogicUpdate state, in Entity e)
        {
            ref var construct = ref e.Get<Construct>();
            ref var pilotable = ref e.Get<ConstructPilotable>();

            pilotable.All.Clear();

            pilotable.PosX.Clear();
            pilotable.NegX.Clear();

            pilotable.PosY.Clear();
            pilotable.NegY.Clear();

            pilotable.PosRot.Clear();
            pilotable.NegRot.Clear();

            foreach (var kvp in construct.Components)
            {
                var (x, y) = kvp.Key;
                var entity = kvp.Value;
                if(entity.Has<Thruster>())
                {
                    var tile = construct[x, y];
                    ref var transform = ref entity.Get<Transform>();
                    ref var thruster = ref entity.Get<Thruster>();
                    var bodyPos = transform.Position;

                    pilotable.All.Add(entity);

                    switch (tile.Orientation)
                    {
                        case ConstructTileOrientation.NORTH:
                            pilotable.NegY.Add(entity);

                            if (IsLessThan(bodyPos.X, 0)) pilotable.PosRot.Add(entity);
                            else if (IsGreaterThan(bodyPos.X, 0)) pilotable.NegRot.Add(entity);

                            break;
                        case ConstructTileOrientation.SOUTH:
                            pilotable.PosY.Add(entity);

                            if (IsLessThan(bodyPos.X, 0)) pilotable.NegRot.Add(entity);
                            else if (IsGreaterThan(bodyPos.X, 0)) pilotable.PosRot.Add(entity);

                            break;
                        case ConstructTileOrientation.EAST:
                            pilotable.NegX.Add(entity);

                            if (IsLessThan(bodyPos.Y, 0)) pilotable.NegRot.Add(entity);
                            else if (IsGreaterThan(bodyPos.Y, 0)) pilotable.PosRot.Add(entity);

                            break;
                        case ConstructTileOrientation.WEST:
                            pilotable.PosX.Add(entity);

                            if (IsLessThan(bodyPos.Y, 0)) pilotable.PosRot.Add(entity);
                            else if (IsGreaterThan(bodyPos.Y, 0)) pilotable.NegRot.Add(entity);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private bool IsLessThan(float num, float target)
        {
            return num < target - 0.05;
        }

        private bool IsGreaterThan(float num, float target)
        {
            return num > target + 0.05;
        }
    }
}
