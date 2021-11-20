using UnityEngine;

[DisallowMultipleComponent]
public class TopNotchOffsetGroup : MonoBehaviour
{
    [SerializeField]
    float notchMargin = -40;

    [SerializeField]
    float notNotchMargin;

    [SerializeField]
    RectTransform rt;

    public bool NotchMarginActive
    {
        get => rt.offsetMax.y < notNotchMargin;
        set => rt.offsetMax = new Vector2(rt.offsetMax.x, value ? notchMargin : notNotchMargin);
    }

    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
}