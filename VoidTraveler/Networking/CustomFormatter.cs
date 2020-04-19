using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace VoidTraveler.Networking
{
    public class CustomResolver : IFormatterResolver
    {
        // Resolver should be singleton.
        public static readonly IFormatterResolver Instance = new CustomResolver();

        private CustomResolver()
        {
        }

        // GetFormatter<T>'s get cost should be minimized so use type cache.
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter;

            // generic's static constructor should be minimized for reduce type generation size!
            // use outer helper method.
            static FormatterCache()
            {
                Formatter = (IMessagePackFormatter<T>)CustomResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }

    internal static class CustomResolverGetFormatterHelper
    {
        // If type is concrete type, use type-formatter map
        static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
        {
            {typeof(Vector2), new Vector2Formatter()},
            {typeof(RgbaFloat), new RgbaFloatFormatter()}
            // add more your own custom serializers.
        };

        internal static object GetFormatter(Type t)
        {
            object formatter;
            if (formatterMap.TryGetValue(t, out formatter))
            {
                return formatter;
            }

            // If target type is generics, use MakeGenericType.
            if (t.IsGenericParameter && t.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                return Activator.CreateInstance(typeof(ValueTupleFormatter<,>).MakeGenericType(t.GenericTypeArguments));
            }

            // If type can not get, must return null for fallback mecanism.
            return null;
        }
    }

    class Vector2Formatter : IMessagePackFormatter<Vector2>
    {
        public Vector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new Vector2((float)reader.ReadDouble(), (float)reader.ReadDouble());
        }

        public void Serialize(ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }
    }

    class RgbaFloatFormatter : IMessagePackFormatter<RgbaFloat>
    {
        public RgbaFloat Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new RgbaFloat((float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
        }

        public void Serialize(ref MessagePackWriter writer, RgbaFloat value, MessagePackSerializerOptions options)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
            writer.Write(value.A);
        }
    }
}
