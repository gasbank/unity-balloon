using System.Collections.Generic;

[System.Serializable]
public class DataSet {
    public Dictionary<ScString, StrBaseData> StrKoData; // 한국어
    public Dictionary<ScString, StrBaseData> StrEnData; // 영어
}
