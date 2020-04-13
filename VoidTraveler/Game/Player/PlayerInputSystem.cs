using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoidTraveler.Game.Player
{
    public class PlayerInputSystem : AEntitySystem<LogicUpdate>
    {

        public PlayerInputSystem(World world) : base(world.GetEntities().With<Player>().AsSet())
        {
        }

        protected override void Update(LogicUpdate update, in Entity entity)
        {
            ref var player = ref entity.Get<Player>();

            player.MoveDirection = Vector2.Zero;
            if(update.Input.GetKey(Tortuga.Platform.TKey.Left))
            {
                player.MoveDirection += Vector2.UnitX * -1f;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.Right))
            {
                player.MoveDirection += Vector2.UnitX;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.Up))
            {
                player.MoveDirection += Vector2.UnitY;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.Down))
            {
                player.MoveDirection += Vector2.UnitY * -1;
            }

            if (update.Input.GetMouseButtonPressed(Tortuga.Platform.TMouseButton.Left))
            {
                player.Fire = true;
                player.FireAt = update.Input.MousePosition;
            }
            else
            {
                player.Fire = false;
            }
        }
    }
}
