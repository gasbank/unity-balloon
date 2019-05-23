using System;
using System.ComponentModel;

public class ScFloatConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        return (ScFloat)float.Parse(value as string);
    }
}

[TypeConverter(typeof(ScFloatConverter))]
[Serializable]
public struct ScFloat
{
    public static readonly int k = 0x28492401;
    public int value;

    public ScFloat(float value) { this.value = BitConverter.ToInt32(BitConverter.GetBytes(value), 0) ^ k; }

    // Implicit conversion from float to ScFloat.
    public static implicit operator ScFloat(float x) { return new ScFloat(x); }

    // Explicit conversion from ScFloat to float.
    public static explicit operator float(ScFloat x) { return BitConverter.ToSingle(BitConverter.GetBytes(x.value ^ k), 0); }

    public static ScFloat operator ++(ScFloat x) { x.value = ((ScFloat)((float)x + 1)).value; return x; }

    public static ScFloat operator --(ScFloat x) { x.value = ((ScFloat)((float)x - 1)).value; return x; }

    public static bool operator ==(ScFloat x, ScFloat y) { return x.value == y.value; }

    public static ScFloat operator +(ScFloat x, ScFloat y) { return ((float)x + (float)y); }

    public static ScFloat operator -(ScFloat x, ScFloat y) { return ((float)x - (float)y); }

    public static ScFloat operator *(ScFloat x, ScFloat y) { return ((float)x * (float)y); }

    public static ScFloat operator /(ScFloat x, ScFloat y) { return ((float)x / (float)y); }

    public static ScFloat operator %(ScFloat x, ScFloat y) { return ((float)x % (float)y); }

    public static bool operator !=(ScFloat x, ScFloat y) { return x.value != y.value; }

    public static bool operator <=(ScFloat x, ScFloat y) { return ((float)x <= (float)y); }

    public static bool operator >=(ScFloat x, ScFloat y) { return ((float)x >= (float)y); }

    public static bool operator <(ScFloat x, ScFloat y) { return ((float)x < (float)y); }

    public static bool operator >(ScFloat x, ScFloat y) { return ((float)x > (float)y); }

    // Overload the conversion from ScFloat to string:
    public static implicit operator string(ScFloat x) { return ((float)x).ToString(); }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScFloat)o).value;
        }
        catch
        {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode() { return value.GetHashCode(); }

    // Override the ToString method to convert DBBool to a string:
    public override string ToString() { return this; }
}
