using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FontManager : MonoBehaviour
{
    public static FontManager instance;

    [SerializeField]
    Font japaneseFont;

    [SerializeField]
    Font koreanFont;

    [SerializeField]
    [FormerlySerializedAs("chineseFont")]
    Font simplifiedChineseFont;

    [SerializeField]
    Font traditionalChineseFont;

    public Font getKoreanFont()
    {
        return koreanFont;
    }

    bool ParseStrRef(string strRef, out string key, out int index)
    {
        key = "";
        index = -1;
        if (strRef.Length < 2) return false;
        if (strRef.StartsWith("\\"))
        {
            var sharpIndex = strRef.LastIndexOf("#");
            if (sharpIndex >= 2 && sharpIndex < strRef.Length - 1)
            {
                key = strRef.Substring(1, sharpIndex - 1);
                if (int.TryParse(strRef.Substring(sharpIndex + 1), out index)) return true;
            }
            else
            {
                // # 뒤의 인덱스가 없으면 #1로 간주
                key = strRef.Substring(1, strRef.Length - 1);
                index = 1;
                return true;
            }
        }

        return false;
    }

    public string ToLocalizedCurrent(string strRef)
    {
        return ToLocalized(strRef, Data.instance.CurrentLanguageCode);
    }

    public string ToLocalizedCurrent(string strRef, params object[] args)
    {
        return ToLocalized(strRef, Data.instance.CurrentLanguageCode, args);
    }

    public string ToLocalized(string strRef, BalloonLanguageCode languageCode, params object[] args)
    {
        var key = "";
        var index = -1;
        var ret = strRef;
        if (ParseStrRef(strRef, out key, out index))
            switch (languageCode)
            {
                case BalloonLanguageCode.En:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrEnData, strRef, languageCode);
                    break;
                case BalloonLanguageCode.Ko:
                default:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrKoData, strRef, languageCode);
                    break;
            }
        else
            Debug.LogWarningFormat("Localized string ref cannot be parsed: {0}", strRef);

        if (args != null && args.Length > 0)
            return string.Format(ret, args);
        return ret;
    }

    string LookupLocalizedDictionary<T>(string key, int index, Dictionary<ScString, T> strBaseData, string strRef,
        BalloonLanguageCode languageCode) where T : StrBaseData
    {
        T strData;
        if (strBaseData.TryGetValue(key, out strData))
        {
            if (index - 1 >= 0 && index - 1 < strData.str.Length)
                return ((string) strData.str[index - 1]).Replace(@"\n", "\n");
            Debug.LogErrorFormat("Invalid localized string index: key:{0} index:{1} on language code {2}.", key, index,
                languageCode);
        }
        else
        {
            Debug.LogWarningFormat("Invalid localized string key: key:{0}/index:{1} on language code {2}", key, index,
                languageCode);
        }

        return strRef;
    }

    public Font GetLanguageFont(BalloonLanguageCode languageCode)
    {
        switch (languageCode)
        {
            case BalloonLanguageCode.Ko:
                return koreanFont;
            case BalloonLanguageCode.Ja:
                return japaneseFont;
            case BalloonLanguageCode.Ch:
                return simplifiedChineseFont;
            case BalloonLanguageCode.Tw:
                return traditionalChineseFont;
            default:
                return koreanFont;
        }
    }
}