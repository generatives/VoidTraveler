using DefaultEcs.System;
using Primitives2D;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.DesktopPlatform;
using Tortuga.Graphics;
using Veldrid;
using Veldrid.StartupUtilities;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Player;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Projectiles;
using DefaultEcs;
using VoidTraveler.Game.Physics;
using VoidTraveler.Physics;
using VoidTraveler.Core;
using System.Collections.Generic;
using VoidTraveler.Game.Constructs.Components;
using System.Linq;
using System.Diagnostics;
using VoidTraveler.Editor;
using VoidTraveler.Networking;
using VoidTraveler.Networking.EntityExistence;
using System.Threading;
using VoidTraveler.Game.Core.Ephemoral;
using System.IO;
using MessagePack;
using MessagePack.Resolvers;

namespace VoidTraveler
{
    class Program
    {
        private static byte _messageId = 0;
        private static Dictionary<int, Action<MemoryStream, Entity>> _recievers = new Dictionary<int, Action<MemoryStream, Entity>>();
        private static Dictionary<Type, Action<object, MemoryStream>> _serializers = new Dictionary<Type, Action<object, MemoryStream>>();
        private static MessagePackSerializerOptions _serializerOptions;

        static void Main(string[] args)
        {
            var resolver = CompositeResolver.Create(CustomResolver.Instance, StandardResolver.Instance);
            _serializerOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            Message<EntityMessage<EntityAdded>>();
            Message<EntityMessage<EntityRemoved>>();
            Message<EntityMessage<TransformMessage>>();
            Message<EntityMessage<PlayerMessage>>();
            Message<EntityMessage<ProjectileMessage>>();
            Message<EntityMessage<PlayerMovementAction>>();
            Message<EntityMessage<PlayerFireAction>>();
            Message<EntityMessage<CameraMessage>>();
            Message<EntityMessage<ConstructMessage>>();
            Message<EntityMessage<ConstructPilotingAction>>();

            var clientScene = new Scene();
            var clientSystems = new List<ISystem<ClientSystemUpdate>>();

            clientScene.RenderingSystems.Add(new ConstructRenderer(clientScene.World));
            clientScene.RenderingSystems.Add(new PlayerRenderer(clientScene.World));
            clientScene.RenderingSystems.Add(new ProjectileRenderer(clientScene.World));

            //clientScene.LogicSystems.Add(new PlayerInputSystem(clientScene.World));
            clientScene.LogicSystems.Add(new EntityAdder(clientScene.World));

            clientScene.LogicSystems.Add(new PlayerStateReciever(clientScene.World));
            clientScene.LogicSystems.Add(new TransformMessageApplier(clientScene.World));
            clientScene.LogicSystems.Add(new ProjectileMessageApplier(clientScene.World));
            clientScene.LogicSystems.Add(new CameraMessageApplier(clientScene.World));
            clientScene.LogicSystems.Add(new ConstructMessageApplier(clientScene.World));

            clientScene.LogicSystems.Add(new TransformLerper(clientScene.World));

            clientScene.LogicSystems.Add(new EntityRemover(clientScene.World));

            clientScene.LogicSystems.Add(new EphemoralEntityRemover(clientScene.World));

            clientSystems.Add(new NetworkedPlayerInputSystem(clientScene.World));
            clientSystems.Add(new NetworkedPlayerFiringSystem(clientScene.World));
            clientSystems.Add(new ConstructPilotingClientSystem(clientScene.World));

            var client = new Client(clientScene, clientSystems, _recievers, _serializers);
            var clientTask = client.Run();

            Task.WaitAll( clientTask);
        }

        private static void Message<T>()
        {
            var messageId = _messageId;
            _recievers[messageId] = (MemoryStream stream, Entity entity) =>
            {
                var message = MessagePackSerializer.Deserialize<T>(stream, _serializerOptions);
                entity.Set(message);
            };
            _serializers[typeof(T)] = (object message, MemoryStream stream) =>
            {
                stream.WriteByte(messageId);
                MessagePackSerializer.Serialize(stream, (T)message, _serializerOptions);
            };
            _messageId++;
        }
    }
}
