using UnityEngine;

public class AchievementReport {
    static public void ReportScore(long value, string code, string text) {
        SocialScoreReporter.instance.QueueScore(code, value, text);
    }
}