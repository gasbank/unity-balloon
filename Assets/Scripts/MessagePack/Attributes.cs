using System;

namespace MessagePack
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MessagePackObjectAttribute : Attribute
    {
        public MessagePackObjectAttribute(bool keyAsPropertyName = false)
        {
            KeyAsPropertyName = keyAsPropertyName;
        }

        public bool KeyAsPropertyName { get; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute(int x)
        {
            IntKey = x;
        }

        public KeyAttribute(string x)
        {
            StringKey = x;
        }

        public int? IntKey { get; }
        public string StringKey { get; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreMemberAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class UnionAttribute : Attribute
    {
        public UnionAttribute(int key, Type subType)
        {
            Key = key;
            SubType = subType;
        }

        public int Key { get; }
        public Type SubType { get; }
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class SerializationConstructorAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
                    AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property)]
    public class MessagePackFormatterAttribute : Attribute
    {
        public MessagePackFormatterAttribute(Type formatterType)
        {
            FormatterType = formatterType;
        }

        public MessagePackFormatterAttribute(Type formatterType, params object[] arguments)
        {
            FormatterType = formatterType;
            Arguments = arguments;
        }

        public Type FormatterType { get; }
        public object[] Arguments { get; }
    }
}