using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

public class VersionText : MonoBehaviour {
    [SerializeField] TMProText text = null;

    void Awake() {
        var info = Resources.Load("App Meta Info") as AppMetaInfo;
        text.text = info.GetAppMetaInfo();
    }
}
