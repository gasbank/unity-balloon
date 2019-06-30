using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionArrowMarkAnimated : MonoBehaviour {
    [SerializeField] Material halfRingMaterialOriginal = null;
    [SerializeField] Renderer halfRingRenderer = null;
    static Material halfRingMaterialCopy = null;

    void Awake() {
        if (halfRingMaterialOriginal != null) {
            if (halfRingMaterialCopy == null) {
                halfRingMaterialCopy = Instantiate(halfRingMaterialOriginal);
            }
        }
        if (halfRingRenderer != null) {
            halfRingRenderer.material = halfRingMaterialCopy;
        }
    }

    void Update() {
        if (halfRingMaterialCopy != null) {
            halfRingMaterialCopy.mainTextureOffset = new Vector2(1.5f * Time.time, 0);
        }
    }
}
