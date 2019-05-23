using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using BigInteger = System.Numerics.BigInteger;

[DisallowMultipleComponent]
public class Data : MonoBehaviour {

    public static Data instance;
    static public DataSet dataSet;

    public BalloonLanguageCode CurrentLanguageCode = BalloonLanguageCode.Ko;
    public bool IsBigNumberNotationOn = false;

    void Awake() {
        if (dataSet == null) {
            var formatter = new BinaryFormatter();
            DataUpdater.DeleteAllCaches();
            using (var s = DataUpdater.GetBuiltInBalloonDataStream()) {
                dataSet = (DataSet)formatter.Deserialize(s);
                s.Close();
            }
            
            PrebuildDependentDataSet(dataSet);
        }
    }

    public static void PrebuildDependentDataSet(DataSet dataSet) {
    }
}
