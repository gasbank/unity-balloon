public class NotSupportedBalloonSaveDataVersionException : System.NotSupportedException {
    public int SaveFileVersion { get; }
    public NotSupportedBalloonSaveDataVersionException(int saveFileVersion) {
        SaveFileVersion = saveFileVersion;
    }
}
