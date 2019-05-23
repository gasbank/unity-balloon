using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BigInteger = System.Numerics.BigInteger;

public static class LocalizedTextUtil {
    public static string Localized(this string str) {
        return FontManager.instance.ToLocalizedCurrent(str);
    }

    public static string Localized(this string str, params object[] args) {
        return FontManager.instance.ToLocalizedCurrent(str, args);
    }

    public static string Localized(this ScString str) {
        return FontManager.instance.ToLocalizedCurrent(str);
    }

    public static string Localized(this ScString str, params object[] args) {
        return FontManager.instance.ToLocalizedCurrent(str, args);
    }

    static readonly int petaPostfixLength = ",000,000,000,000,000".Length;
    static readonly int teraPostfixLength = ",000,000,000,000".Length;
    static readonly int gigaPostfixLength = ",000,000,000".Length;
    static readonly int megaPostfixLength = ",000,000".Length;
    static readonly int kiloPostfixLength = ",000".Length;

    public static readonly string[] koreanCurrencyPostfixList = new [] { null, null, null, null, "만", null, null, null, "억", null, null, null, "조", null, null, null, "경", null, null, null, "해", null, null, null, "자", null };
    public static readonly string[] japaneseCurrencyPostfixList = new [] { null, null, null, null, "万", null, null, null, "億", null, null, null, "兆", null, null, null, "京", null, null, null, "垓", null, null, null, "じょ", null };
    public static readonly string[] chineseCurrencyPostfixList = new [] { null, null, null, null, "万", null, null, null, "亿", null, null, null, "兆", null, null, null, "京", null, null, null, "垓", null, null, null, "秭", null };
    public static readonly string[] taiwanCurrencyPostfixList = new [] { null, null, null, null, "萬", null, null, null, "億", null, null, null, "兆", null, null, null, "京", null, null, null, "垓", null, null, null, "秭", null };
    public static readonly Dictionary<BalloonLanguageCode, string[]> currencyPostfixListDict = new Dictionary<BalloonLanguageCode, string[]>() {
        { BalloonLanguageCode.Ko, koreanCurrencyPostfixList },
        { BalloonLanguageCode.Ja, japaneseCurrencyPostfixList },
        { BalloonLanguageCode.Ch, chineseCurrencyPostfixList },
        { BalloonLanguageCode.Tw, taiwanCurrencyPostfixList },
    };
    public static readonly CultureInfo DefaultCultureInfo = new CultureInfo("en-US");

    public static string Postfixed(this System.IFormattable formattable) {
        if (Data.instance.IsBigNumberNotationOn && currencyPostfixListDict.TryGetValue(Data.instance.CurrentLanguageCode, out var currencyPostfixList)) {
            return PostfixedForCurrency(formattable, currencyPostfixList);
        } else {
            return formattable.ToString("n0", DefaultCultureInfo);
        }
    }

    private static string PostfixedForCurrency(System.IFormattable bigInteger, string[] currencyPostfixList) {
        var s = bigInteger.ToString();
        var sLen = s.Length;
        var sb = new StringBuilder();
        var omitZero = false;
        var postfixAppended = false;
        var postfixCount = 0;
        for (int i = 0; i < s.Length; i++) {
            if (s[i] != '0' || omitZero == false) {
                if (postfixAppended) {
                    sb.Append(' ');
                    postfixAppended = false;
                }
                sb.Append(s[i]);
                omitZero = false;
            }

            var irev = (s.Length - i) - 1;
            var postfixStr = currencyPostfixList[irev < currencyPostfixList.Length ? irev : currencyPostfixList.Length - 1];
            if (postfixStr != null && omitZero == false) {
                sb.Append(postfixStr);
                omitZero = true;
                postfixCount++;
                if (postfixCount >= 3) {
                    break;
                }
                postfixAppended = true;
            }
        }
        return sb.ToString();
    }
}
