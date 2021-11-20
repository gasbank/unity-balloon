using System;
using MessagePack.Formatters;
#if !UNITY_WSA
using System.Reflection;
using MessagePack.Internal;

namespace MessagePack.Resolvers
{
    public sealed class DynamicEnumAsStringResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new DynamicEnumAsStringResolver();

        DynamicEnumAsStringResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var ti = typeof(T).GetTypeInfo();

                if (ti.IsNullable())
                {
                    // build underlying type and use wrapped formatter.
                    ti = ti.GenericTypeArguments[0].GetTypeInfo();
                    if (!ti.IsEnum) return;

                    var innerFormatter = Instance.GetFormatterDynamic(ti.AsType());
                    if (innerFormatter == null) return;
                    formatter = (IMessagePackFormatter<T>) Activator.CreateInstance(
                        typeof(StaticNullableFormatter<>).MakeGenericType(ti.AsType()), innerFormatter);
                    return;
                }

                if (!ti.IsEnum)
                {
                    return;
                }

                formatter = new EnumAsStringFormatter<T>();
            }
        }
    }
}

#endif