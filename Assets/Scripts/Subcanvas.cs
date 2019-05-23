using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public class Subcanvas : MonoBehaviour {
    [SerializeField] Canvas canvas = null;
    public bool IsOpen { get { return canvas.enabled; } }

    void OnValidate() {
        canvas = GetComponent<Canvas>();
    }

    void SendPopupEventMessage() {
        SendMessage(canvas.enabled ? "OpenPopup" : "ClosePopup");
    }

    public void Toggle() {
        canvas.enabled = !canvas.enabled;
        SendPopupEventMessage();
    }

    public void Open() {
        if (canvas.enabled == false) {
            canvas.enabled = true;
            SendPopupEventMessage();
        }
    }

    public void Close() {
        if (canvas.enabled) {
            canvas.enabled = false;
            SendPopupEventMessage();
        }
    }
}
