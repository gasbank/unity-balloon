using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ShortMessage : MonoBehaviour {
    public static ShortMessage instance;
    public Text text;
    public float remainTime = 0;
    public float alpha = 1.0f;
    public float alphaVel;
    public Image image;
    public float visibleTime = 2.0f;
    public float bgMaxAlpha = 0.5f;

    void Awake() {
        image = GetComponent<Image>();
    }

    void Update() {
        if (remainTime < 0) {
            alpha = Mathf.SmoothDamp(alpha, 0, ref alphaVel, 0.1f);
            if (alpha < 1e-2) {
                gameObject.SetActive(false);
            }
        } else {
            remainTime -= Time.deltaTime;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, bgMaxAlpha * alpha);
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }

    public void ShowLocalized(string message) {
        Show(("\\" + message).Localized());
    }

    public void Show(string message) {
        gameObject.SetActive(true);
        text.text = message;
        remainTime = visibleTime;
        alpha = 1.0f;
        Sound.instance.PlayError();
    }
}
