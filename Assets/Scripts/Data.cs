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
            try {
                // 온라인에서 받은 데이터시트가 있다면 그것을 쓰고,
                // 없다면 빌트인된 데이터시트를 쓴다.
                using (var s = DataUpdater.GetBalloonDataStream()) {
                    dataSet = (DataSet)formatter.Deserialize(s);
                    s.Close();
                }
            } catch (System.Exception e) {
                // 어떤 연유에서인지 이 단계가 실패하면,
                // (아마도) 온라인에서 받은 데이터가 문제일 확률이 높다.
                // 빌트인된 데이터시트로 다시 시도한다.
                Debug.LogException(e);
                DataUpdater.DeleteAllCaches();
                using (var s = DataUpdater.GetBuiltInBalloonDataStream()) {
                    dataSet = (DataSet)formatter.Deserialize(s);
                    s.Close();
                }
            }
            
            PrebuildDependentDataSet(dataSet);
        }
    }

    public static void PrebuildDependentDataSet(DataSet dataSet) {
    }
}
