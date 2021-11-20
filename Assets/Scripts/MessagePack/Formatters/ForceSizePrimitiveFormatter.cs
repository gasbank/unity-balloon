namespace MessagePack.Formatters
{
    public sealed class ForceInt16BlockFormatter : IMessagePackFormatter<short>
    {
        public static readonly ForceInt16BlockFormatter Instance = new ForceInt16BlockFormatter();

        ForceInt16BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, short value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt16ForceInt16Block(ref bytes, offset, value);
        }

        public short Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadInt16(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceInt16BlockFormatter : IMessagePackFormatter<short?>
    {
        public static readonly NullableForceInt16BlockFormatter Instance = new NullableForceInt16BlockFormatter();

        NullableForceInt16BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, short? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteInt16ForceInt16Block(ref bytes, offset, value.Value);
        }

        public short? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadInt16(bytes, offset, out readSize);
        }
    }

    public sealed class ForceInt16BlockArrayFormatter : IMessagePackFormatter<short[]>
    {
        public static readonly ForceInt16BlockArrayFormatter Instance = new ForceInt16BlockArrayFormatter();

        ForceInt16BlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, short[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteInt16ForceInt16Block(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public short[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new short[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadInt16(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ForceInt32BlockFormatter : IMessagePackFormatter<int>
    {
        public static readonly ForceInt32BlockFormatter Instance = new ForceInt32BlockFormatter();

        ForceInt32BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, int value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32ForceInt32Block(ref bytes, offset, value);
        }

        public int Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceInt32BlockFormatter : IMessagePackFormatter<int?>
    {
        public static readonly NullableForceInt32BlockFormatter Instance = new NullableForceInt32BlockFormatter();

        NullableForceInt32BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, int? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteInt32ForceInt32Block(ref bytes, offset, value.Value);
        }

        public int? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class ForceInt32BlockArrayFormatter : IMessagePackFormatter<int[]>
    {
        public static readonly ForceInt32BlockArrayFormatter Instance = new ForceInt32BlockArrayFormatter();

        ForceInt32BlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, int[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteInt32ForceInt32Block(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public int[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new int[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ForceInt64BlockFormatter : IMessagePackFormatter<long>
    {
        public static readonly ForceInt64BlockFormatter Instance = new ForceInt64BlockFormatter();

        ForceInt64BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, long value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt64ForceInt64Block(ref bytes, offset, value);
        }

        public long Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadInt64(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceInt64BlockFormatter : IMessagePackFormatter<long?>
    {
        public static readonly NullableForceInt64BlockFormatter Instance = new NullableForceInt64BlockFormatter();

        NullableForceInt64BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, long? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteInt64ForceInt64Block(ref bytes, offset, value.Value);
        }

        public long? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadInt64(bytes, offset, out readSize);
        }
    }

    public sealed class ForceInt64BlockArrayFormatter : IMessagePackFormatter<long[]>
    {
        public static readonly ForceInt64BlockArrayFormatter Instance = new ForceInt64BlockArrayFormatter();

        ForceInt64BlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, long[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteInt64ForceInt64Block(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public long[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new long[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ForceUInt16BlockFormatter : IMessagePackFormatter<ushort>
    {
        public static readonly ForceUInt16BlockFormatter Instance = new ForceUInt16BlockFormatter();

        ForceUInt16BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ushort value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt16ForceUInt16Block(ref bytes, offset, value);
        }

        public ushort Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadUInt16(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceUInt16BlockFormatter : IMessagePackFormatter<ushort?>
    {
        public static readonly NullableForceUInt16BlockFormatter Instance = new NullableForceUInt16BlockFormatter();

        NullableForceUInt16BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ushort? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteUInt16ForceUInt16Block(ref bytes, offset, value.Value);
        }

        public ushort? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadUInt16(bytes, offset, out readSize);
        }
    }

    public sealed class ForceUInt16BlockArrayFormatter : IMessagePackFormatter<ushort[]>
    {
        public static readonly ForceUInt16BlockArrayFormatter Instance = new ForceUInt16BlockArrayFormatter();

        ForceUInt16BlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ushort[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteUInt16ForceUInt16Block(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public ushort[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new ushort[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadUInt16(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ForceUInt32BlockFormatter : IMessagePackFormatter<uint>
    {
        public static readonly ForceUInt32BlockFormatter Instance = new ForceUInt32BlockFormatter();

        ForceUInt32BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, uint value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt32ForceUInt32Block(ref bytes, offset, value);
        }

        public uint Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceUInt32BlockFormatter : IMessagePackFormatter<uint?>
    {
        public static readonly NullableForceUInt32BlockFormatter Instance = new NullableForceUInt32BlockFormatter();

        NullableForceUInt32BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, uint? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteUInt32ForceUInt32Block(ref bytes, offset, value.Value);
        }

        public uint? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
        }
    }

    public sealed class ForceUInt32BlockArrayFormatter : IMessagePackFormatter<uint[]>
    {
        public static readonly ForceUInt32BlockArrayFormatter Instance = new ForceUInt32BlockArrayFormatter();

        ForceUInt32BlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, uint[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteUInt32ForceUInt32Block(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public uint[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new uint[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ForceUInt64BlockFormatter : IMessagePackFormatter<ulong>
    {
        public static readonly ForceUInt64BlockFormatter Instance = new ForceUInt64BlockFormatter();

        ForceUInt64BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ulong value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt64ForceUInt64Block(ref bytes, offset, value);
        }

        public ulong Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceUInt64BlockFormatter : IMessagePackFormatter<ulong?>
    {
        public static readonly NullableForceUInt64BlockFormatter Instance = new NullableForceUInt64BlockFormatter();

        NullableForceUInt64BlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ulong? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteUInt64ForceUInt64Block(ref bytes, offset, value.Value);
        }

        public ulong? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
        }
    }

    public sealed class ForceUInt64BlockArrayFormatter : IMessagePackFormatter<ulong[]>
    {
        public static readonly ForceUInt64BlockArrayFormatter Instance = new ForceUInt64BlockArrayFormatter();

        ForceUInt64BlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ulong[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteUInt64ForceUInt64Block(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public ulong[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new ulong[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ForceByteBlockFormatter : IMessagePackFormatter<byte>
    {
        public static readonly ForceByteBlockFormatter Instance = new ForceByteBlockFormatter();

        ForceByteBlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, byte value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteByteForceByteBlock(ref bytes, offset, value);
        }

        public byte Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadByte(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceByteBlockFormatter : IMessagePackFormatter<byte?>
    {
        public static readonly NullableForceByteBlockFormatter Instance = new NullableForceByteBlockFormatter();

        NullableForceByteBlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, byte? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteByteForceByteBlock(ref bytes, offset, value.Value);
        }

        public byte? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadByte(bytes, offset, out readSize);
        }
    }


    public sealed class ForceSByteBlockFormatter : IMessagePackFormatter<sbyte>
    {
        public static readonly ForceSByteBlockFormatter Instance = new ForceSByteBlockFormatter();

        ForceSByteBlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, sbyte value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteSByteForceSByteBlock(ref bytes, offset, value);
        }

        public sbyte Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadSByte(bytes, offset, out readSize);
        }
    }

    public sealed class NullableForceSByteBlockFormatter : IMessagePackFormatter<sbyte?>
    {
        public static readonly NullableForceSByteBlockFormatter Instance = new NullableForceSByteBlockFormatter();

        NullableForceSByteBlockFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, sbyte? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteSByteForceSByteBlock(ref bytes, offset, value.Value);
        }

        public sbyte? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadSByte(bytes, offset, out readSize);
        }
    }

    public sealed class ForceSByteBlockArrayFormatter : IMessagePackFormatter<sbyte[]>
    {
        public static readonly ForceSByteBlockArrayFormatter Instance = new ForceSByteBlockArrayFormatter();

        ForceSByteBlockArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, sbyte[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteSByteForceSByteBlock(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public sbyte[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new sbyte[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadSByte(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }
}