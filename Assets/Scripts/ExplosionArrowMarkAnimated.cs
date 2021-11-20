using UnityEngine;

public class ExplosionArrowMarkAnimated : MonoBehaviour
{
    static Material halfRingMaterialCopy;

    [SerializeField]
    Material halfRingMaterialOriginal;

    [SerializeField]
    Renderer halfRingRenderer;

    void Awake()
    {
        if (halfRingMaterialOriginal != null)
            if (halfRingMaterialCopy == null)
                halfRingMaterialCopy = Instantiate(halfRingMaterialOriginal);
        if (halfRingRenderer != null) halfRingRenderer.material = halfRingMaterialCopy;
    }

    void Update()
    {
        if (halfRingMaterialCopy != null) halfRingMaterialCopy.mainTextureOffset = new Vector2(1.5f * Time.time, 0);
    }
}