public class AchievementReport
{
    public static void ReportScore(long value, string code, string text)
    {
        SocialScoreReporter.instance.QueueScore(code, value, text);
    }
}