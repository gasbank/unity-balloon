using System;
using System.Runtime.InteropServices;

namespace MessagePack
{
    // safe accessor of Single/Double's underlying byte.
    // This code is borrowed from MsgPack-Cli https://github.com/msgpack/msgpack-cli

    [StructLayout(LayoutKind.Explicit)]
    internal struct Float32Bits
    {
        [FieldOffset(0)]
        public readonly float Value;

        [FieldOffset(0)]
        public readonly byte Byte0;

        [FieldOffset(1)]
        public readonly byte Byte1;

        [FieldOffset(2)]
        public readonly byte Byte2;

        [FieldOffset(3)]
        public readonly byte Byte3;

        public Float32Bits(float value)
        {
            this = default;
            Value = value;
        }

        public Float32Bits(byte[] bigEndianBytes, int offset)
        {
            this = default;

            if (BitConverter.IsLittleEndian)
            {
                Byte0 = bigEndianBytes[offset + 3];
                Byte1 = bigEndianBytes[offset + 2];
                Byte2 = bigEndianBytes[offset + 1];
                Byte3 = bigEndianBytes[offset];
            }
            else
            {
                Byte0 = bigEndianBytes[offset];
                Byte1 = bigEndianBytes[offset + 1];
                Byte2 = bigEndianBytes[offset + 2];
                Byte3 = bigEndianBytes[offset + 3];
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct Float64Bits
    {
        [FieldOffset(0)]
        public readonly double Value;

        [FieldOffset(0)]
        public readonly byte Byte0;

        [FieldOffset(1)]
        public readonly byte Byte1;

        [FieldOffset(2)]
        public readonly byte Byte2;

        [FieldOffset(3)]
        public readonly byte Byte3;

        [FieldOffset(4)]
        public readonly byte Byte4;

        [FieldOffset(5)]
        public readonly byte Byte5;

        [FieldOffset(6)]
        public readonly byte Byte6;

        [FieldOffset(7)]
        public readonly byte Byte7;

        public Float64Bits(double value)
        {
            this = default;
            Value = value;
        }

        public Float64Bits(byte[] bigEndianBytes, int offset)
        {
            this = default;

            if (BitConverter.IsLittleEndian)
            {
                Byte0 = bigEndianBytes[offset + 7];
                Byte1 = bigEndianBytes[offset + 6];
                Byte2 = bigEndianBytes[offset + 5];
                Byte3 = bigEndianBytes[offset + 4];
                Byte4 = bigEndianBytes[offset + 3];
                Byte5 = bigEndianBytes[offset + 2];
                Byte6 = bigEndianBytes[offset + 1];
                Byte7 = bigEndianBytes[offset];
            }
            else
            {
                Byte0 = bigEndianBytes[offset];
                Byte1 = bigEndianBytes[offset + 1];
                Byte2 = bigEndianBytes[offset + 2];
                Byte3 = bigEndianBytes[offset + 3];
                Byte4 = bigEndianBytes[offset + 4];
                Byte5 = bigEndianBytes[offset + 5];
                Byte6 = bigEndianBytes[offset + 6];
                Byte7 = bigEndianBytes[offset + 7];
            }
        }
    }
}