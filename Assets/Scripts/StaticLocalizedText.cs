using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

[DisallowMultipleComponent]
public class StaticLocalizedText : MonoBehaviour
{
    [SerializeField]
    TMProText text;

    [field: SerializeField]
    public string StrRef { get; set; } = "";

    public static string ToLiteral(string input)
    {
        return input.Replace("\n", @"\n");
    }

    void OnValidate()
    {
        UpdateStrRef();
    }

    void UpdateStrRef()
    {
        text = GetComponent<TMProText>();
        StrRef = ToLiteral(text.text);
    }

    void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = Data.dataSet != null ? FontManager.instance.ToLocalizedCurrent("\\" + StrRef) : StrRef;
    }
}