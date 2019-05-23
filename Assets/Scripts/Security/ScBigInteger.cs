using System;
using System.ComponentModel;
using BigInteger = System.Numerics.BigInteger;

public class ScBigIntegerConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        return (ScBigInteger)BigInteger.Parse(value as string);
    }
}

[TypeConverter(typeof(ScBigIntegerConverter))]
[Serializable]
public struct ScBigInteger
{
    public static readonly BigInteger k = 0x2375489130230113;
    public BigInteger value;

    public ScBigInteger(BigInteger value) { this.value = value ^ k; }
    public ScBigInteger(string value) { this.value = BigInteger.Parse(value) ^ k; }

    // Implicit conversion from BigInteger to ScBigInteger.
    public static implicit operator ScBigInteger(BigInteger x) { return new ScBigInteger(x); }
    // Implicit conversion from long to ScBigInteger.
    public static implicit operator ScBigInteger(long x) { return new ScBigInteger(x); }

    // Explicit conversion from ScBigInteger to BigInteger.
    public static implicit operator BigInteger(ScBigInteger x) { return x.value ^ k; }

    public static ScBigInteger operator ++(ScBigInteger x) { x.value = ((ScBigInteger)((BigInteger)x + 1)).value; return x; }

    public static ScBigInteger operator --(ScBigInteger x) { x.value = ((ScBigInteger)((BigInteger)x - 1)).value; return x; }

    public static bool operator ==(ScBigInteger x, ScBigInteger y) { return x.value == y.value; }

    public static ScBigInteger operator +(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x + (BigInteger)y); }

    public static ScBigInteger operator -(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x - (BigInteger)y); }

    public static ScBigInteger operator *(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x * (BigInteger)y); }

    public static ScBigInteger operator /(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x / (BigInteger)y); }

    public static ScBigInteger operator %(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x % (BigInteger)y); }

    public static bool operator !=(ScBigInteger x, ScBigInteger y) { return x.value != y.value; }

    public static bool operator <=(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x <= (BigInteger)y); }

    public static bool operator >=(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x >= (BigInteger)y); }

    public static bool operator <(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x < (BigInteger)y); }

    public static bool operator >(ScBigInteger x, ScBigInteger y) { return ((BigInteger)x > (BigInteger)y); }

    // Overload the conversion from ScBigInteger to string:
    public static implicit operator string(ScBigInteger x) { return ((BigInteger)x).ToString(); }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScBigInteger)o).value;
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

    public BigInteger ToBigInteger() { return this; }

    // Make ScBigInteger as OrderBy-able using LINQ
    public int CompareTo(ScBigInteger other) { return ((BigInteger)this).CompareTo(other); }
}
