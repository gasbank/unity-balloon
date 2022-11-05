using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public class AndroidKeyLoader : IPreprocessBuildWithReport
{
    static AndroidKeyLoader()
    {
        TryLoadKey();
    }

    static void TryLoadKey()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        try
        {
            var key = File.ReadAllText(".keystore_pass").Trim();
            PlayerSettings.Android.keystorePass = key;
            PlayerSettings.Android.keyaliasPass = key;
        }
        catch (FileNotFoundException)
        {
            // skip
        }
    }

    public int callbackOrder { get; }
    public void OnPreprocessBuild(BuildReport report)
    {
        TryLoadKey();
    }
}
