using DefaultEcs;
using DefaultEcs.System;
using Ruffles.Channeling;
using Ruffles.Configuration;
using Ruffles.Connections;
using Ruffles.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTraveler.Editor;
using VoidTraveler.Game.Core.Ephemoral;
using VoidTraveler.Networking;

namespace VoidTraveler
{
    public class Server : NetworkedRuntime
    {
        private List<ISystem<ServerSystemUpdate>> _serverSystems;

        private SocketConfig _serverConfig = new SocketConfig()
        {
            ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
            ChannelTypes = new ChannelType[]
            {
                ChannelType.Reliable,
                ChannelType.ReliableSequenced,
                ChannelType.Unreliable,
                ChannelType.UnreliableOrdered,
                ChannelType.ReliableSequencedFragmented
            },
            DualListenPort = 5674,
            SimulatorConfig = new Ruffles.Simulation.SimulatorConfig()
            {
                DropPercentage = 0.05f,
                MaxLatency = 10,
                MinLatency = 0
            },
            UseSimulator = false
        };

        private RuffleSocket _server;
        private List<Connection> _newConnections;
        private List<Connection> _connections;
        private ulong _messagesSent;

        public Server(Scene scene, List<ISystem<ServerSystemUpdate>> serverSystems, Dictionary<int, Action<MemoryStream, Entity>> recievers, Dictionary<Type, Action<object, MemoryStream>> serializers) :
            base(scene, recievers, serializers)
        {
            _serverSystems = serverSystems;
            _newConnections = new List<Connection>();
            _connections = new List<Connection>();
        }

        public Task Run()
        {
            _server = new RuffleSocket(_serverConfig);
            _server.Start();

            return Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Update();
                }
            }, TaskCreationOptions.LongRunning);
        }

        protected override void PreUpdate(double deltaTime)
        {
            base.PreUpdate(deltaTime);

            //InfoViewer.Values["Server FPS"] = Math.Round(1f / deltaTime, 2).ToString();

            NetworkEvent networkEvent = _server.Poll();
            while (networkEvent.Type != NetworkEventType.Nothing)
            {
                switch (networkEvent.Type)
                {
                    case NetworkEventType.Connect:
                        _newConnections.Add(networkEvent.Connection);
                        break;
                    case NetworkEventType.Data:
                        MessageRecieved(networkEvent.Data);
                        break;
                }
                networkEvent.Recycle();
                networkEvent = _server.Poll();
            }
        }

        protected override void PostUpdate(double deltaTime)
        {
            var serverUpdate = new ServerSystemUpdate()
            {
                DeltaTime = deltaTime,
                Messages = new List<object>(),
                NewClients = _newConnections.Any(),
                NewClientMessages = new List<object>()
            };

            foreach (var system in _serverSystems)
            {
                system.Update(serverUpdate);
            }

            if(_newConnections.Any())
            {
                var newConnectionMessage = SerializeMessages(serverUpdate.NewClientMessages);

                foreach (var conn in _newConnections)
                {
                    conn.Send(newConnectionMessage, 4, false, _messagesSent++);
                }
            }

            var message = SerializeMessages(serverUpdate.Messages);

            foreach (var conn in _connections)
            {
                conn.Send(message, 4, false, _messagesSent++);
            }

            _connections.AddRange(_newConnections);
            _newConnections.Clear();

            while (Stopwatch.Elapsed.TotalSeconds < 0.05)
            {
                Thread.Sleep(5);
            }
        }
    }
}
