using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ShortMessage : MonoBehaviour
{
    public static ShortMessage instance;
    public float alpha = 1.0f;
    public float alphaVel;
    public float bgMaxAlpha = 0.5f;
    public Image image;
    public float remainTime;
    public TextMeshProUGUI text;
    public float visibleTime = 2.0f;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (remainTime < 0)
        {
            alpha = Mathf.SmoothDamp(alpha, 0, ref alphaVel, 0.1f);
            if (alpha < 1e-2) gameObject.SetActive(false);
        }
        else
        {
            remainTime -= Time.deltaTime;
        }

        if (image != null)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, bgMaxAlpha * alpha);
        }
        
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }

    public void ShowLocalized(string message)
    {
        Show(("\\" + message).Localized());
    }

    public void Show(string message)
    {
        gameObject.SetActive(true);
        text.text = message;
        remainTime = visibleTime;
        alpha = 1.0f;
        BalloonSound.instance.PlayError();
    }
}