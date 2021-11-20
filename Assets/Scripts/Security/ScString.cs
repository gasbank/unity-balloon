using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

public class ScStringConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        return (ScString) (value as string);
    }
}

[TypeConverter(typeof(ScStringConverter))]
[Serializable]
public class ScString
{
    public static readonly byte[] k = {0x19, 0x87, 0x06, 0x22};
    public byte[] value;

    public ScString(byte[] value)
    {
        this.value = value;
    }

    public ScString(string value)
    {
        this.value = Encoding.UTF8.GetBytes(value);
        for (var i = 0; i < this.value.Length; i++) this.value[i] ^= k[i % k.Length];
    }

    static bool ByteArrayCompare(byte[] a1, byte[] a2)
    {
        if (a1 == a2)
            return true;
        if (a1 == null && a2 != null)
            return false;
        if (a1 != null && a2 == null)
            return false;

        if (a1.Length != a2.Length)
            return false;

        for (var i = 0; i < a1.Length; i++)
            if (a1[i] != a2[i])
                return false;

        return true;
    }

    // Implicit conversion from string to ScString.
    public static implicit operator ScString(string x)
    {
        return new ScString(x);
    }

    public static bool operator ==(ScString x, ScString y)
    {
        return ByteArrayCompare(x?.value ?? null, y?.value ?? null);
    }

    public static bool operator !=(ScString x, ScString y)
    {
        return !(x == y);
    }

    // Overload the conversion from ScString to string:
    public static implicit operator string(ScString x)
    {
        var xClone = (byte[]) x.value.Clone();
        for (var i = 0; i < x.value.Length; i++) xClone[i] ^= k[i % k.Length];
        return Encoding.UTF8.GetString(xClone);
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return ByteArrayCompare(value, ((ScString) o).value);
        }
        catch
        {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    // Override the ToString method to convert DBBool to a string:
    public override string ToString()
    {
        return this;
    }
}