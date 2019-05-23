using UnityEngine;
using UnityEngine.UI;
using TMProText = TMPro.TextMeshProUGUI;

[DisallowMultipleComponent]
public class StaticLocalizedText : MonoBehaviour {
    [SerializeField] private TMProText text = null;
    [SerializeField] private string strRef = "";
    public string StrRef { get { return strRef; } }

    public static string ToLiteral(string input) {
        return input.Replace("\n", @"\n");
    }

    private void OnValidate() {
        UpdateStrRef();
    }

    private void UpdateStrRef() {
        text = GetComponent<TMProText>();
        strRef = ToLiteral(text.text);
    }

    void OnEnable() {
        UpdateText();
    }

    public void UpdateText() {
        text.text = Data.dataSet != null ? FontManager.instance.ToLocalizedCurrent("\\" + strRef) : strRef;
    }
}
