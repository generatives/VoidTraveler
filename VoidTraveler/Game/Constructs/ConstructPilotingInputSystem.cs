using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Constructs
{
    [MessagePackObject]
    public struct ConstructPilotingAction
    {
        [Key(0)]
        public bool Forward { get; set; }
        [Key(1)]
        public bool Backward { get; set; }
        [Key(2)]
        public bool Left { get; set; }
        [Key(3)]
        public bool Right { get; set; }
        [Key(4)]
        public bool RotateLeft { get; set; }
        [Key(5)]
        public bool RotateRight { get; set; }
    }

    [With(typeof(Player.Player), typeof(NetworkedEntity))]
    public class ConstructPilotingClientSystem : AEntitySystem<ClientSystemUpdate>
    {
        public ConstructPilotingClientSystem(World world) : base(world)
        {
        }

        protected override void Update(ClientSystemUpdate state, in Entity entity)
        {
            var action = new ConstructPilotingAction()
            {
                Forward = state.Input.GetKey(Tortuga.Platform.TKey.W),
                Backward = state.Input.GetKey(Tortuga.Platform.TKey.S),
                Left = state.Input.GetKey(Tortuga.Platform.TKey.A),
                Right = state.Input.GetKey(Tortuga.Platform.TKey.D),
                RotateLeft = state.Input.GetKey(Tortuga.Platform.TKey.Q),
                RotateRight = state.Input.GetKey(Tortuga.Platform.TKey.E),
            };

            var player = entity.Get<Player.Player>();
            state.Messages.Add(new EntityMessage<ConstructPilotingAction>(player.CurrentConstruct.Get<NetworkedEntity>().Id, action));
        }
    }

    public class ConstructPilotingApplier : EntityMessageApplier<LogicUpdate, ConstructPilotingAction>
    {
        public ConstructPilotingApplier(NetworkedEntities entities, World world) : base(entities, world) { }

        protected override void Update(LogicUpdate state, in ConstructPilotingAction messageData, in Entity entity)
        {
            entity.Set(new ConstructPilotable()
            {
                Forward = messageData.Forward,
                Backward = messageData.Backward,
                Left = messageData.Left,
                Right = messageData.Right,
                RotateLeft = messageData.RotateLeft,
                RotateRight = messageData.RotateRight,
            });
        }
    }
}
