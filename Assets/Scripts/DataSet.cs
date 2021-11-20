using System;
using System.Collections.Generic;

[Serializable]
public class DataSet
{
    public Dictionary<ScString, StrBaseData> StrEnData; // 영어
    public Dictionary<ScString, StrBaseData> StrKoData; // 한국어
}