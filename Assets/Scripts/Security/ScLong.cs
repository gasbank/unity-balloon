using System;
using System.ComponentModel;

public class ScLongConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        var v = value as string;
        if (v.ToUpperInvariant().Contains("E+")) {
            var vd = double.Parse(v);
            if (vd >= long.MinValue && vd <= long.MaxValue) {
                return (ScLong)(long)vd;
            } else {
                throw new ArgumentOutOfRangeException();
            }
        } else {
            return (ScLong)long.Parse(v);
        }
    }
}

[TypeConverter(typeof(ScLongConverter))]
[Serializable]
public struct ScLong
{
    public static readonly long k = 0x5582010239348484;
    public long value;

    public ScLong(long value) { this.value = value ^ k; }

    // Implicit conversion from long to ScLong.
    public static implicit operator ScLong(long x) { return new ScLong(x); }

    // Explicit conversion from ScLong to long.
    public static implicit operator long(ScLong x) { return x.value ^ k; }

    public static ScLong operator ++(ScLong x) { x.value = ((ScLong)((long)x + 1)).value; return x; }

    public static ScLong operator --(ScLong x) { x.value = ((ScLong)((long)x - 1)).value; return x; }

    public static bool operator ==(ScLong x, ScLong y) { return x.value == y.value; }

    public static ScLong operator +(ScLong x, ScLong y) { return ((long)x + (long)y); }

    public static ScLong operator -(ScLong x, ScLong y) { return ((long)x - (long)y); }

    public static ScLong operator *(ScLong x, ScLong y) { return ((long)x * (long)y); }

    public static ScLong operator /(ScLong x, ScLong y) { return ((long)x / (long)y); }

    public static ScLong operator %(ScLong x, ScLong y) { return ((long)x % (long)y); }

    public static bool operator !=(ScLong x, ScLong y) { return x.value != y.value; }

    public static bool operator <=(ScLong x, ScLong y) { return ((long)x <= (long)y); }

    public static bool operator >=(ScLong x, ScLong y) { return ((long)x >= (long)y); }

    public static bool operator <(ScLong x, ScLong y) { return ((long)x < (long)y); }

    public static bool operator >(ScLong x, ScLong y) { return ((long)x > (long)y); }

    // Overload the conversion from ScLong to string:
    public static implicit operator string(ScLong x) { return ((long)x).ToString(); }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScLong)o).value;
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

    // Override the ToString method to convert DBBool to a string:
    public string ToString(string format) { return ToLong().ToString(format); }

    public long ToLong() { return this; }

    // Make ScLong as OrderBy-able using LINQ
    public int CompareTo(ScLong other) { return ((long)this).CompareTo(other); }
}
