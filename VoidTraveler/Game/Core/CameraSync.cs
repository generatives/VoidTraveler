﻿using DefaultEcs;
using DefaultEcs.System;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using VoidTraveler.Networking;

namespace VoidTraveler.Game.Core
{
    [MessagePackObject]
    public struct CameraMessage
    {
    }

    [With(typeof(Camera), typeof(NetworkedEntity))]
    public class CameraServerSystem : AEntitySystem<ServerSystemUpdate>
    {
        public CameraServerSystem(World world) : base(world)
        {
        }

        protected override void Update(ServerSystemUpdate update, in Entity entity)
        {
            if(update.NewClients)
            {
                var id = entity.Get<NetworkedEntity>().Id;
                update.NewClientMessages.Add(new EntityMessage<CameraMessage>() { Id = id, Data = new CameraMessage() });
            }
        }
    }

    public class CameraMessageApplier : EntityMessageApplier<CameraMessage>
    {
        public CameraMessageApplier(NetworkedEntities entities) : base(entities) { }

        protected override void On(in CameraMessage action, in Entity entity)
        {
            entity.Set(new Camera());
        }
    }
}
