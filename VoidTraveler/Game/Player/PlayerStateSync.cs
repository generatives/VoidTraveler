using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Player
{
    [MessagePackObject]
    public struct PlayerMessage
    {
        [Key(0)]
        public float Radius { get; set; }
        [Key(1)]
        public RgbaFloat Colour { get; set; }
        [Key(2)]
        public Guid CurrentConstruct { get; set; }
    }

    [With(typeof(Player), typeof(NetworkedEntity))]
    public class PlayerStateServerSystem : AEntitySystem<ServerSystemUpdate>
    {
        public PlayerStateServerSystem(World world) : base(world) { }

        protected override void Update(ServerSystemUpdate state, in Entity entity)
        {
            if(state.NewClients)
            {
                var id = entity.Get<NetworkedEntity>().Id;
                var player = entity.Get<Player>();
                state.NewClientMessages.Add(new EntityMessage<PlayerMessage>(id, new PlayerMessage()
                {
                    Radius = player.Radius,
                    Colour = player.Colour,
                    CurrentConstruct = player.CurrentConstruct.Get<NetworkedEntity>().Id
                }));
            }
        }
    }

    public class PlayerStateReciever : EntityMessageApplier<LogicUpdate, PlayerMessage>
    {
        public PlayerStateReciever(World world) : base(world) { }

        protected override void Update(LogicUpdate state, in PlayerMessage messageData, in Entity entity)
        {
            entity.Set(new Player() { Radius = messageData.Radius, Colour = messageData.Colour, CurrentConstruct = Entities[new NetworkedEntity() { Id = messageData.CurrentConstruct }] });
        }
    }
}
