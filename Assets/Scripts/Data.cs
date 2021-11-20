using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[DisallowMultipleComponent]
public class Data : MonoBehaviour
{
    public static Data instance;
    public static DataSet dataSet;

    public BalloonLanguageCode CurrentLanguageCode = BalloonLanguageCode.Ko;
    public bool IsBigNumberNotationOn;

    void ChangeLanguageBySystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean: //korean
                CurrentLanguageCode = BalloonLanguageCode.Ko;
                break;
            default:
                CurrentLanguageCode = BalloonLanguageCode.En;
                break;
        }
    }

    void Awake()
    {
        ChangeLanguageBySystemLanguage();

        if (dataSet == null)
        {
            var formatter = new BinaryFormatter();
            DataUpdater.DeleteAllCaches();
            using (var s = DataUpdater.GetBuiltInBalloonDataStream())
            {
                dataSet = (DataSet) formatter.Deserialize(s);
                s.Close();
            }

            PrebuildDependentDataSet(dataSet);
        }
    }

    public static void PrebuildDependentDataSet(DataSet dataSet)
    {
    }
}