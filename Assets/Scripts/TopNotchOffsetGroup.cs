using UnityEngine;

[DisallowMultipleComponent]
public class TopNotchOffsetGroup : MonoBehaviour {
    [SerializeField] private RectTransform rt;
    [SerializeField] private float notNotchMargin = 0;
    [SerializeField] private float notchMargin = -40;
    private void OnValidate() {
        rt = GetComponent<RectTransform>();
    }
    public bool NotchMarginActive { get { return rt.offsetMax.y < notNotchMargin; } set { rt.offsetMax = new Vector2(rt.offsetMax.x, value ? notchMargin : notNotchMargin); } }
}
