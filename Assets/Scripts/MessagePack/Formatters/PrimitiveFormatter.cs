using System;

namespace MessagePack.Formatters
{
    public sealed class Int16Formatter : IMessagePackFormatter<short>
    {
        public static readonly Int16Formatter Instance = new Int16Formatter();

        Int16Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, short value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt16(ref bytes, offset, value);
        }

        public short Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadInt16(bytes, offset, out readSize);
        }
    }

    public sealed class NullableInt16Formatter : IMessagePackFormatter<short?>
    {
        public static readonly NullableInt16Formatter Instance = new NullableInt16Formatter();

        NullableInt16Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, short? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteInt16(ref bytes, offset, value.Value);
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

    public sealed class Int16ArrayFormatter : IMessagePackFormatter<short[]>
    {
        public static readonly Int16ArrayFormatter Instance = new Int16ArrayFormatter();

        Int16ArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, short[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteInt16(ref bytes, offset, value[i]);

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

    public sealed class Int32Formatter : IMessagePackFormatter<int>
    {
        public static readonly Int32Formatter Instance = new Int32Formatter();

        Int32Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, int value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, value);
        }

        public int Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class NullableInt32Formatter : IMessagePackFormatter<int?>
    {
        public static readonly NullableInt32Formatter Instance = new NullableInt32Formatter();

        NullableInt32Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, int? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteInt32(ref bytes, offset, value.Value);
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

    public sealed class Int32ArrayFormatter : IMessagePackFormatter<int[]>
    {
        public static readonly Int32ArrayFormatter Instance = new Int32ArrayFormatter();

        Int32ArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, int[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteInt32(ref bytes, offset, value[i]);

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

    public sealed class Int64Formatter : IMessagePackFormatter<long>
    {
        public static readonly Int64Formatter Instance = new Int64Formatter();

        Int64Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, long value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt64(ref bytes, offset, value);
        }

        public long Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadInt64(bytes, offset, out readSize);
        }
    }

    public sealed class NullableInt64Formatter : IMessagePackFormatter<long?>
    {
        public static readonly NullableInt64Formatter Instance = new NullableInt64Formatter();

        NullableInt64Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, long? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteInt64(ref bytes, offset, value.Value);
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

    public sealed class Int64ArrayFormatter : IMessagePackFormatter<long[]>
    {
        public static readonly Int64ArrayFormatter Instance = new Int64ArrayFormatter();

        Int64ArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, long[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteInt64(ref bytes, offset, value[i]);

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

    public sealed class UInt16Formatter : IMessagePackFormatter<ushort>
    {
        public static readonly UInt16Formatter Instance = new UInt16Formatter();

        UInt16Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ushort value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt16(ref bytes, offset, value);
        }

        public ushort Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadUInt16(bytes, offset, out readSize);
        }
    }

    public sealed class NullableUInt16Formatter : IMessagePackFormatter<ushort?>
    {
        public static readonly NullableUInt16Formatter Instance = new NullableUInt16Formatter();

        NullableUInt16Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ushort? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteUInt16(ref bytes, offset, value.Value);
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

    public sealed class UInt16ArrayFormatter : IMessagePackFormatter<ushort[]>
    {
        public static readonly UInt16ArrayFormatter Instance = new UInt16ArrayFormatter();

        UInt16ArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ushort[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteUInt16(ref bytes, offset, value[i]);

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

    public sealed class UInt32Formatter : IMessagePackFormatter<uint>
    {
        public static readonly UInt32Formatter Instance = new UInt32Formatter();

        UInt32Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, uint value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt32(ref bytes, offset, value);
        }

        public uint Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
        }
    }

    public sealed class NullableUInt32Formatter : IMessagePackFormatter<uint?>
    {
        public static readonly NullableUInt32Formatter Instance = new NullableUInt32Formatter();

        NullableUInt32Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, uint? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteUInt32(ref bytes, offset, value.Value);
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

    public sealed class UInt32ArrayFormatter : IMessagePackFormatter<uint[]>
    {
        public static readonly UInt32ArrayFormatter Instance = new UInt32ArrayFormatter();

        UInt32ArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, uint[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value[i]);

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

    public sealed class UInt64Formatter : IMessagePackFormatter<ulong>
    {
        public static readonly UInt64Formatter Instance = new UInt64Formatter();

        UInt64Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ulong value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt64(ref bytes, offset, value);
        }

        public ulong Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
        }
    }

    public sealed class NullableUInt64Formatter : IMessagePackFormatter<ulong?>
    {
        public static readonly NullableUInt64Formatter Instance = new NullableUInt64Formatter();

        NullableUInt64Formatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ulong? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteUInt64(ref bytes, offset, value.Value);
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

    public sealed class UInt64ArrayFormatter : IMessagePackFormatter<ulong[]>
    {
        public static readonly UInt64ArrayFormatter Instance = new UInt64ArrayFormatter();

        UInt64ArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, ulong[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteUInt64(ref bytes, offset, value[i]);

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

    public sealed class SingleFormatter : IMessagePackFormatter<float>
    {
        public static readonly SingleFormatter Instance = new SingleFormatter();

        SingleFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, float value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteSingle(ref bytes, offset, value);
        }

        public float Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadSingle(bytes, offset, out readSize);
        }
    }

    public sealed class NullableSingleFormatter : IMessagePackFormatter<float?>
    {
        public static readonly NullableSingleFormatter Instance = new NullableSingleFormatter();

        NullableSingleFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, float? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteSingle(ref bytes, offset, value.Value);
        }

        public float? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadSingle(bytes, offset, out readSize);
        }
    }

    public sealed class SingleArrayFormatter : IMessagePackFormatter<float[]>
    {
        public static readonly SingleArrayFormatter Instance = new SingleArrayFormatter();

        SingleArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, float[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteSingle(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public float[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new float[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class DoubleFormatter : IMessagePackFormatter<double>
    {
        public static readonly DoubleFormatter Instance = new DoubleFormatter();

        DoubleFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, double value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteDouble(ref bytes, offset, value);
        }

        public double Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadDouble(bytes, offset, out readSize);
        }
    }

    public sealed class NullableDoubleFormatter : IMessagePackFormatter<double?>
    {
        public static readonly NullableDoubleFormatter Instance = new NullableDoubleFormatter();

        NullableDoubleFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, double? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteDouble(ref bytes, offset, value.Value);
        }

        public double? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadDouble(bytes, offset, out readSize);
        }
    }

    public sealed class DoubleArrayFormatter : IMessagePackFormatter<double[]>
    {
        public static readonly DoubleArrayFormatter Instance = new DoubleArrayFormatter();

        DoubleArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, double[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteDouble(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public double[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new double[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadDouble(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class BooleanFormatter : IMessagePackFormatter<bool>
    {
        public static readonly BooleanFormatter Instance = new BooleanFormatter();

        BooleanFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, bool value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteBoolean(ref bytes, offset, value);
        }

        public bool Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
        }
    }

    public sealed class NullableBooleanFormatter : IMessagePackFormatter<bool?>
    {
        public static readonly NullableBooleanFormatter Instance = new NullableBooleanFormatter();

        NullableBooleanFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, bool? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteBoolean(ref bytes, offset, value.Value);
        }

        public bool? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
        }
    }

    public sealed class BooleanArrayFormatter : IMessagePackFormatter<bool[]>
    {
        public static readonly BooleanArrayFormatter Instance = new BooleanArrayFormatter();

        BooleanArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, bool[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public bool[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new bool[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class ByteFormatter : IMessagePackFormatter<byte>
    {
        public static readonly ByteFormatter Instance = new ByteFormatter();

        ByteFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, byte value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteByte(ref bytes, offset, value);
        }

        public byte Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadByte(bytes, offset, out readSize);
        }
    }

    public sealed class NullableByteFormatter : IMessagePackFormatter<byte?>
    {
        public static readonly NullableByteFormatter Instance = new NullableByteFormatter();

        NullableByteFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, byte? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteByte(ref bytes, offset, value.Value);
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


    public sealed class SByteFormatter : IMessagePackFormatter<sbyte>
    {
        public static readonly SByteFormatter Instance = new SByteFormatter();

        SByteFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, sbyte value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteSByte(ref bytes, offset, value);
        }

        public sbyte Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadSByte(bytes, offset, out readSize);
        }
    }

    public sealed class NullableSByteFormatter : IMessagePackFormatter<sbyte?>
    {
        public static readonly NullableSByteFormatter Instance = new NullableSByteFormatter();

        NullableSByteFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, sbyte? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteSByte(ref bytes, offset, value.Value);
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

    public sealed class SByteArrayFormatter : IMessagePackFormatter<sbyte[]>
    {
        public static readonly SByteArrayFormatter Instance = new SByteArrayFormatter();

        SByteArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, sbyte[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteSByte(ref bytes, offset, value[i]);

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

    public sealed class CharFormatter : IMessagePackFormatter<char>
    {
        public static readonly CharFormatter Instance = new CharFormatter();

        CharFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, char value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteChar(ref bytes, offset, value);
        }

        public char Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadChar(bytes, offset, out readSize);
        }
    }

    public sealed class NullableCharFormatter : IMessagePackFormatter<char?>
    {
        public static readonly NullableCharFormatter Instance = new NullableCharFormatter();

        NullableCharFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, char? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteChar(ref bytes, offset, value.Value);
        }

        public char? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadChar(bytes, offset, out readSize);
        }
    }

    public sealed class CharArrayFormatter : IMessagePackFormatter<char[]>
    {
        public static readonly CharArrayFormatter Instance = new CharArrayFormatter();

        CharArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, char[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++) offset += MessagePackBinary.WriteChar(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public char[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new char[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadChar(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }

    public sealed class DateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        public static readonly DateTimeFormatter Instance = new DateTimeFormatter();

        DateTimeFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, DateTime value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteDateTime(ref bytes, offset, value);
        }

        public DateTime Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return MessagePackBinary.ReadDateTime(bytes, offset, out readSize);
        }
    }

    public sealed class NullableDateTimeFormatter : IMessagePackFormatter<DateTime?>
    {
        public static readonly NullableDateTimeFormatter Instance = new NullableDateTimeFormatter();

        NullableDateTimeFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, DateTime? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
                return MessagePackBinary.WriteNil(ref bytes, offset);
            return MessagePackBinary.WriteDateTime(ref bytes, offset, value.Value);
        }

        public DateTime? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            return MessagePackBinary.ReadDateTime(bytes, offset, out readSize);
        }
    }

    public sealed class DateTimeArrayFormatter : IMessagePackFormatter<DateTime[]>
    {
        public static readonly DateTimeArrayFormatter Instance = new DateTimeArrayFormatter();

        DateTimeArrayFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, DateTime[] value, IFormatterResolver formatterResolver)
        {
            if (value == null) return MessagePackBinary.WriteNil(ref bytes, offset);

            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
            for (var i = 0; i < value.Length; i++)
                offset += MessagePackBinary.WriteDateTime(ref bytes, offset, value[i]);

            return offset - startOffset;
        }

        public DateTime[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;

            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;
            var array = new DateTime[len];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = MessagePackBinary.ReadDateTime(bytes, offset, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return array;
        }
    }
}