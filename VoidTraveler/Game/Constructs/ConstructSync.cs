﻿using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Constructs
{
    [MessagePackObject]
    public struct ConstructMessage
    {
        [Key(0)]
        public int XLength;
        [Key(1)]
        public int YLength;
        [Key(2)]
        public float TileSize;
        [Key(3)]
        public ConstructTile[] Tiles;
    }

    [With(typeof(NetworkedEntity))]
    [WhenChangedEither(typeof(Construct))]
    [WhenAddedEither(typeof(Construct))]
    public class ConstructServerSystem : AEntitySystem<ServerSystemUpdate>
    {
        public ConstructServerSystem(World world) : base(world) { }

        protected override void Update(ServerSystemUpdate state, in Entity entity)
        {
            var construct = entity.Get<Construct>();
            ref var netEntity = ref entity.Get<NetworkedEntity>();

            var tilesCopy = new ConstructTile[construct.Tiles.Length];
            construct.Tiles.CopyTo(tilesCopy, 0);

            state.Messages.Add(new EntityMessage<ConstructMessage>(netEntity.Id, new ConstructMessage()
            {
                XLength = construct.XLength,
                YLength = construct.YLength,
                TileSize = construct.TileSize,
                Tiles = tilesCopy,
            }));
        }
    }

    [With(typeof(NetworkedEntity), typeof(Construct))]
    public class ConstructServerInitSystem : AEntitySystem<ServerSystemUpdate>
    {
        public ConstructServerInitSystem(World world) : base(world) { }

        protected override void Update(ServerSystemUpdate state, in Entity entity)
        {
            if (state.NewClients)
            {
                var construct = entity.Get<Construct>();
                ref var netEntity = ref entity.Get<NetworkedEntity>();

                var tilesCopy = new ConstructTile[construct.Tiles.Length];
                construct.Tiles.CopyTo(tilesCopy, 0);

                state.NewClientMessages.Add(new EntityMessage<ConstructMessage>(netEntity.Id, new ConstructMessage()
                {
                    XLength = construct.XLength,
                    YLength = construct.YLength,
                    TileSize = construct.TileSize,
                    Tiles = tilesCopy,
                }));
            }
        }
    }

    public class ConstructMessageApplier : EntityMessageApplier<ConstructMessage>
    {
        public ConstructMessageApplier(NetworkedEntities entities) : base(entities) { }

        protected override void On(in ConstructMessage messageData, in Entity entity)
        {
            var construct = entity.Has<Construct>() ?
                entity.Get<Construct>() :
                new Construct(messageData.XLength, messageData.YLength, messageData.TileSize);

            construct.TileSize = messageData.TileSize;
            messageData.Tiles.CopyTo(construct.Tiles, 0);

            entity.Set(construct);
        }
    }
}
