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

            var serverScene = new Scene();
            var serverSystems = new List<ISystem<ServerSystemUpdate>>();

            serverSystems.Add(new EntityExistenceSender(serverScene.World));
            serverSystems.Add(new TransformInitServerSystem(serverScene.World));
            serverSystems.Add(new TransformChangeServerSystem(serverScene.World));
            serverSystems.Add(new PlayerStateServerSystem(serverScene.World));
            serverSystems.Add(new ProjectileServerSystem(serverScene.World));
            serverSystems.Add(new CameraServerSystem(serverScene.World));
            serverSystems.Add(new ConstructServerSystem(serverScene.World));

            serverScene.RenderingSystems.Add(new ConstructRenderer(serverScene.World));
            serverScene.RenderingSystems.Add(new PlayerRenderer(serverScene.World));
            serverScene.RenderingSystems.Add(new ProjectileRenderer(serverScene.World));

            var physicsSystem = new PhysicsSystem();
            serverScene.LogicSystems.Add(new ConstructBodyGenerator(physicsSystem, serverScene.World));
            serverScene.LogicSystems.Add(physicsSystem);
            serverScene.LogicSystems.Add(new PhysicsBodySync(serverScene.World));
            serverScene.LogicSystems.Add(new ConstructPilotingApplier(serverScene.World));
            serverScene.LogicSystems.Add(new ConstructPilotSystem(serverScene.World));

            serverScene.LogicSystems.Add(new NetworkedPlayerInputReciever(serverScene.World));
            serverScene.LogicSystems.Add(new NetworkedPlayerFiringReciever(serverScene.World));
            serverScene.LogicSystems.Add(new PlayerMovementSystem(physicsSystem, serverScene.World));

            serverScene.LogicSystems.Add(new ProjectileMovementSystem(physicsSystem, serverScene.World));
            serverScene.LogicSystems.Add(new EphemoralEntityRemover(serverScene.World));

            var construct = serverScene.World.CreateEntity();
            construct.Set(new NetworkedEntity() { Id = Guid.NewGuid() });
            construct.Set(new Transform() { Position = new Vector2(0, 0) });
            construct.Set(CreateBoundedConstruct(1, 10, 20));
            construct.Set(new ConstructPilotable());
            construct.Set(new PhysicsBody());
            construct.Set(new Camera());

            var otherConstruct = serverScene.World.CreateEntity();
            otherConstruct.Set(new NetworkedEntity() { Id = Guid.NewGuid() });
            otherConstruct.Set(new Transform() { Position = new Vector2(30, 0) });
            otherConstruct.Set(CreateBoundedConstruct(1, 10, 10));
            otherConstruct.Set(new PhysicsBody());

            var player = serverScene.World.CreateEntity();
            player.Set(new NetworkedEntity() { Id = Guid.NewGuid() });
            player.Set(new Transform() { Position = new Vector2(0, 0), Parent = construct });
            player.Set(new Player() { Radius = 0.5f, Colour = RgbaFloat.Blue, MoveSpeed = 5, CurrentConstruct = construct });

            var server = new Server(serverScene, serverSystems, _recievers, _serializers);

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

            var serverTask = server.Run();
            var clientTask = client.Run();

            Task.WaitAll(serverTask, clientTask);
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

        private static Construct CreateBoundedConstruct(float tileSize, int width, int height)
        {
            var construct = new Construct(width, height, tileSize);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var edge = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    construct[x, y] = new ConstructTile() { Exists = true, Collides = edge, Colour = edge ? RgbaFloat.Grey : RgbaFloat.White };
                }
            }

            return construct;
        }
    }
}
