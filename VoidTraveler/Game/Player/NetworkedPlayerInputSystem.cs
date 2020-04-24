using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoidTraveler.Game.Core;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Player
{
    [MessagePackObject]
    public struct PlayerMovementAction
    {
        [Key(0)]
        public Vector2 MoveDirection { get; set; }
    }

    [With(typeof(Player), typeof(NetworkedEntity))]
    public class NetworkedPlayerInputSystem : AEntitySystem<ClientSystemUpdate>
    {
        public NetworkedPlayerInputSystem(World world) : base(world)
        {
        }

        protected override void Update(ClientSystemUpdate update, in Entity entity)
        {
            var inputMessage = new PlayerMovementAction();

            inputMessage.MoveDirection = Vector2.Zero;
            if (update.Input.GetKey(Tortuga.Platform.TKey.Left))
            {
                inputMessage.MoveDirection += Vector2.UnitX * -1f;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.Right))
            {
                inputMessage.MoveDirection += Vector2.UnitX;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.Up))
            {
                inputMessage.MoveDirection += Vector2.UnitY;
            }
            if (update.Input.GetKey(Tortuga.Platform.TKey.Down))
            {
                inputMessage.MoveDirection += Vector2.UnitY * -1;
            }

            var id = entity.Get<NetworkedEntity>().Id;
            update.Messages.Add(new EntityMessage<PlayerMovementAction>() { Id = id, Data = inputMessage });
        }
    }

    public class NetworkedPlayerInputReciever : EntityMessageApplier<PlayerMovementAction>
    {
        public NetworkedPlayerInputReciever(NetworkedEntities entities) : base(entities) { }

        protected override void On(in PlayerMovementAction action, in Entity entity)
        {
            if (entity.Has<Player>())
            {
                ref var player = ref entity.Get<Player>();

                player.MoveDirection = action.MoveDirection;

                entity.Set(player);
            }
        }
    }

    [MessagePackObject]
    public struct PlayerFireAction
    {
        [Key(0)]
        public Vector2 FireAt { get; set; }
    }

    [With(typeof(Player), typeof(NetworkedEntity))]
    public class NetworkedPlayerFiringSystem : AEntitySystem<ClientSystemUpdate>
    {
        public NetworkedPlayerFiringSystem(World world) : base(world)
        {
        }

        protected override void Update(ClientSystemUpdate update, in Entity entity)
        {
            if (update.Input.GetMouseButtonPressed(Tortuga.Platform.TMouseButton.Left))
            {
                var action = new PlayerFireAction();
                action.FireAt = update.Input.MousePosition;

                var id = entity.Get<NetworkedEntity>().Id;
                update.Messages.Add(new EntityMessage<PlayerFireAction>() { Id = id, Data = action });
            }
        }
    }

    public class NetworkedPlayerFiringReciever : EntityMessageApplier<PlayerFireAction>
    {
        public NetworkedPlayerFiringReciever(NetworkedEntities entities) : base(entities) { }

        protected override void On(in PlayerFireAction action, in Entity entity)
        {
            if(entity.Has<Player>())
            {
                ref var player = ref entity.Get<Player>();
                player.Fire = true;
                player.FireAt = action.FireAt;

                entity.Set(player);
            }
        }
    }
}
