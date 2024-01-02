using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialScoreReporter : MonoBehaviour
{
    public static SocialScoreReporter instance;
    static bool notifyCheatModeOnlyOnce;
    readonly Dictionary<string, long> queuedScoreDict = new Dictionary<string, long>();
    readonly Dictionary<string, long> successfullyReportedScoreDict = new Dictionary<string, long>();

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);
            // 1초 간격으로 리포트하니까 그 사이 queuedScoreDict가 바뀌어 있을 수도 있다.
            // 복사해서 써야한다.
            foreach (var kv in new Dictionary<string, long>(queuedScoreDict))
            {
                BalloonDebug.Log($"Social.ReportScore {kv.Key}: {kv.Value}...");
                if (!Application.isEditor)
                {
                    Social.ReportScore(kv.Value, kv.Key, success =>
                    {
                        BalloonDebug.Log($"Social.ReportScore {kv.Key}: {kv.Value} (Result:{success})");
                        if (success) successfullyReportedScoreDict[kv.Key] = kv.Value;
                    });
                }
                else
                {
                    BalloonDebug.Log($"Social.ReportScore {kv.Key}: {kv.Value} (Result:EDITOR MODE)");
                    successfullyReportedScoreDict[kv.Key] = kv.Value;
                }

                yield return new WaitForSeconds(1.0f);
            }

            queuedScoreDict.Clear();
        }
    }

    public void QueueScore(string key, long value, string desc)
    {
        if (Application.isEditor)
        {
            //BalloonDebug.Log($"Editor Mode: Try to queue score... {key}: {value} ({desc})");
        }

        long oldValue = 0;
        if (successfullyReportedScoreDict.TryGetValue(key, out oldValue) && oldValue == value)
        {
            // 앱이 실행된 이후 성공적으로 등록했던 점수와 동일하다면 다시 할 필요는 없다.
        }
        else
        {
            if (BalloonSpawner.instance.cheatMode)
            {
                if (notifyCheatModeOnlyOnce == false)
                {
                    BalloonDebug.Log("### Cheat Mode was turned on (notified only at the first time)");
                    BalloonDebug.Log($"ReportScore: {key}: {value} ({desc})");
                    notifyCheatModeOnlyOnce = true;
                }

                return;
            }

            queuedScoreDict[key] = value;
        }
    }
}