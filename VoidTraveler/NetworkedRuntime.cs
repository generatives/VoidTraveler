﻿using DefaultEcs;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VoidTraveler.Editor;
using VoidTraveler.Game.Core.Ephemoral;
using VoidTraveler.Scenes;

namespace VoidTraveler
{
    public class NetworkedRuntime : Runtime
    {
        private Dictionary<int, Action<MemoryStream, World>> _messageRecievers;
        private Dictionary<Type, Action<object, MemoryStream>> _messageSerializer;

        public NetworkedRuntime(Scene scene, Dictionary<int, Action<MemoryStream, World>> recievers, Dictionary<Type, Action<object, MemoryStream>> serializers) : base(scene)
        {
            _messageRecievers = recievers;
            _messageSerializer = serializers;
        }

        public byte[] SerializeMessages(List<object> messages)
        {
            using(var stream = new MemoryStream())
            {
                byte[] lengthBytes = BitConverter.GetBytes(messages.Count);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                stream.Write(lengthBytes, 0, 4);

                foreach(var message in messages)
                {
                    _messageSerializer[message.GetType()](message, stream);
                }

                return stream.ToArray();
            }
        }

        public void MessageRecieved(ArraySegment<byte> message)
        {
            using(var stream = new MemoryStream(message.Array, message.Offset, message.Count))
            {
                var lengthBytes = new byte[4];
                stream.Read(lengthBytes, 0, 4);
                if(BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                var length = BitConverter.ToInt32(lengthBytes, 0);

                for (int i = 0; i < length; i++)
                {
                    var messageType = stream.ReadByte();
                    _messageRecievers[messageType](stream, Scene.World);
                }
            }
        }
    }
}
