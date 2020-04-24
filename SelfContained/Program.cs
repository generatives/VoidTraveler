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
using VoidTraveler.Scenes;
using Singularity;

namespace VoidTraveler
{
    class Program
    {
        private static byte _messageId = 0;
        private static Dictionary<int, Action<MemoryStream, World>> _recievers = new Dictionary<int, Action<MemoryStream, World>>();
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

            var serverContainer = new Container(builder =>
            {
                builder.Register<World>(c => c.With(Lifetimes.PerContainer));
                builder.Register<FarseerPhysics.Dynamics.World>(c => c.Inject(() => new FarseerPhysics.Dynamics.World(Vector2.Zero)).With(Lifetimes.PerContainer));

                builder.Register<IMessageReciever, ConstructPilotingApplier>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, NetworkedPlayerInputReciever>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, NetworkedPlayerFiringReciever>(c => c.With(Lifetimes.PerContainer));

                builder.Register<ISystem<LogicUpdate>, ConstructBodyGenerator>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<LogicUpdate>, PhysicsSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<LogicUpdate>, PhysicsBodySync>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<LogicUpdate>, ConstructPilotSystem>(c => c.With(Lifetimes.PerContainer));

                builder.Register<ISystem<LogicUpdate>, PlayerMovementSystem>(c => c.With(Lifetimes.PerContainer));

                builder.Register<ISystem<LogicUpdate>, ProjectileMovementSystem>(c => c.With(Lifetimes.PerContainer));

                builder.Register<NetworkedEntities>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, EntityExistenceSender>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, TransformInitServerSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, TransformChangeServerSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, PlayerStateServerSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, ProjectileServerSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, CameraServerSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ServerSystemUpdate>, ConstructServerSystem>(c => c.With(Lifetimes.PerContainer));
            });

            var serverScene = new Scene(
                serverContainer.GetInstance<World>(),
                serverContainer.GetInstance<List<ISystem<LogicUpdate>>>(),
                serverContainer.GetInstance<List<ISystem<DrawDevice>>>()
                );

            var server = new Server(serverScene, serverContainer.GetInstance<List<ISystem<ServerSystemUpdate>>>(), _recievers, _serializers);

            var serverRecievers = serverContainer.GetInstance<List<IMessageReciever>>();
            serverRecievers.Select(r => serverScene.World.Subscribe(r)).ToList();

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

            var clientContainer = new Container((builder) =>
            {
                builder.Register<World>(c => c.With(Lifetimes.PerContainer));

                builder.Register<ISystem<DrawDevice>, ConstructRenderer>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<DrawDevice>, PlayerRenderer>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<DrawDevice>, ProjectileRenderer>(c => c.With(Lifetimes.PerContainer));

                builder.Register<NetworkedEntities>(c => c.With(Lifetimes.PerContainer));

                builder.Register<IMessageReciever, EntityAdder>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, PlayerStateReciever>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, TransformMessageApplier>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, ProjectileMessageApplier>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, CameraMessageApplier>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, ConstructMessageApplier>(c => c.With(Lifetimes.PerContainer));
                builder.Register<IMessageReciever, EntityRemover>(c => c.With(Lifetimes.PerContainer));

                builder.Register<ISystem<LogicUpdate>, TransformLerper>(c => c.With(Lifetimes.PerContainer));

                builder.Register<ISystem<ClientSystemUpdate>, NetworkedPlayerInputSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ClientSystemUpdate>, NetworkedPlayerFiringSystem>(c => c.With(Lifetimes.PerContainer));
                builder.Register<ISystem<ClientSystemUpdate>, ConstructPilotingClientSystem>(c => c.With(Lifetimes.PerContainer));
            });

            var clientScene = new Scene(
                clientContainer.GetInstance<World>(),
                clientContainer.GetInstance<List<ISystem<LogicUpdate>>>(),
                clientContainer.GetInstance<List<ISystem<DrawDevice>>>()
                );

            var client = new Client(clientScene, clientContainer.GetInstance<List<ISystem<ClientSystemUpdate>>>(), _recievers, _serializers);

            var recievers = clientContainer.GetInstance<List<IMessageReciever>>();
            var disp = recievers.Select(r => clientScene.World.Subscribe(r)).ToList();

            var serverTask = server.Run();
            client.Run();
        }

        private static void Message<T>()
        {
            var messageId = _messageId;
            _recievers[messageId] = (MemoryStream stream, World world) =>
            {
                var message = MessagePackSerializer.Deserialize<T>(stream, _serializerOptions);
                world.Publish(in message);
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
