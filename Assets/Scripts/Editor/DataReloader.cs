using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class DataReloader {
    [MenuItem("Balloon/Reload Data")]
    static void ReloadData() {
        var dataSet = new DataSet();

        var xlsxList = new string[] {
            "Data/StrKo.xlsx",
            "Data/StrEn.xlsx",
        };
        foreach (var xlsx in xlsxList) {
            var tableList = new ExcelToObject.TableList(xlsx);
            tableList.MapInto(dataSet);
        }

        IFormatter formatter = new BinaryFormatter();
        var balloonDataPath = "Assets/Resources/Data/Balloon.bytes";
        using (Stream stream = new FileStream(balloonDataPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
           formatter.Serialize(stream, dataSet);
           stream.Close();
        }
    }

    // https://stackoverflow.com/questions/323640/can-i-convert-a-c-sharp-string-value-to-an-escaped-string-literal
    public static string ToLiteral(string input) {
        using (var writer = new StringWriter()) {
            using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
                provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                return writer.ToString();
            }
        }
    }

    [MenuItem("Balloon/Generate textref.txt")]
    static void GatherAllStaticLocalizedTextRef() {
        List<string> textRefList = new List<string>();
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) {
            foreach (var staticLocalizedText in root.GetComponentsInChildren<StaticLocalizedText>(true)) {
                //var strRefLiteral = ToLiteral(staticLocalizedText.StrRef);
                //textRefList.Add(strRefLiteral.Substring(1, strRefLiteral.Length - 2));
                textRefList.Add(staticLocalizedText.StrRef);
            }
        }
        textRefList = textRefList.Distinct().ToList();
        textRefList.Sort();
        File.WriteAllLines("textref.txt", textRefList.ToArray());
        SushiDebug.LogFormat("textref.txt written: {0} items", textRefList.Count);
    }

    [MenuItem("Balloon/Print All Text Sizes")]
    static void PrintAllTextSizes() {
        List<string> lines = new List<string>();
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) {
            foreach (var text in root.GetComponentsInChildren<Text>(true)) {
                lines.Add($"{text.fontSize}\t{text.gameObject.name}\t{text.text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")}");
            }
        }
        File.WriteAllLines("all-text-sizes.txt", lines.OrderBy(e => e));
    }

    [MenuItem("Balloon/Check Local Scales")]
    static void CheckLocalScales() {
        bool firstTime = true;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
                if (t.GetComponent<Canvas>() != null) {
                    continue;
                }
                if (t.localScale.x != 1 && t.localScale.x != -1) {
                    if (firstTime) {
                        ClearLog();
                        firstTime = false;
                    }
                    Debug.LogError($"Abs(Scale X) != 1 check error: {t.name} (Value={t.localScale.x})", t);
                } else if (t.localScale.y != 1 && t.localScale.y != -1) {
                    if (firstTime) {
                        ClearLog();
                        firstTime = false;
                    }
                    Debug.LogError($"Abs(Scale Y) != 1 check error: {t.name} (Value={t.localScale.y})", t);
                } else if (t.localScale.z != 1 && t.localScale.z != -1) {
                    if (firstTime) {
                        ClearLog();
                        firstTime = false;
                    }
                    Debug.LogError($"Abs(Scale Z) = 1 check error: {t.name} (Value={t.localScale.z})", t);
                }
            }
        }
    }

    // https://stackoverflow.com/questions/40577412/clear-editor-console-logs-from-script
    public static void ClearLog() {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}
