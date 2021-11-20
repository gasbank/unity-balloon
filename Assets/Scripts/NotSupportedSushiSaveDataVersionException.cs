using System;

public class NotSupportedBalloonSaveDataVersionException : NotSupportedException
{
    public NotSupportedBalloonSaveDataVersionException(int saveFileVersion)
    {
        SaveFileVersion = saveFileVersion;
    }

    public int SaveFileVersion { get; }
}