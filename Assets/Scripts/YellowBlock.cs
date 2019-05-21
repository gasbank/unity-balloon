using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMProText = TMPro.TextMeshPro;

public class YellowBlock : MonoBehaviour {
    [SerializeField] int hp = 3;
    [SerializeField] float lastHitTime = 0;
    [SerializeField] float lastHitMinInterval = 0.5f;
    [SerializeField] TMProText hpText = null;
    [SerializeField] GameObject afterDestroySpawn = null;
    [SerializeField] Transform afterDestroySpawnParent = null;
    [SerializeField] Material halfRingMaterialOriginal = null;
    [SerializeField] Renderer halfRingRenderer = null;
    static Material halfRingMaterialCopy = null;

    void Awake() {
        hpText.text = hp.ToString();
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
            halfRingMaterialCopy.mainTextureOffset = new Vector2(0, Time.time);
        }
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log($"YellowBlock collision enter: relativeVelocity magnitude: {collision.relativeVelocity.magnitude}");
        var player = collision.gameObject.GetComponentInParent<HotairBalloon>();
        if (Time.time - lastHitTime > lastHitMinInterval && player != null && player.IsGameOver == false) {
            lastHitTime = Time.time;
            hp--;
            hpText.text = hp.ToString();
            player.AddExplosionForce((collision.transform.position - collision.contacts[0].point).normalized);
            player.IncreaseFeverGauge();
            if (hp <= 0) {
                if (afterDestroySpawn != null) {
                    if (afterDestroySpawnParent != null) {
                        Instantiate(afterDestroySpawn, afterDestroySpawnParent.position, afterDestroySpawnParent.rotation);
                    } else {
                        Instantiate(afterDestroySpawn, transform.position, transform.rotation);
                    }
                }
                Destroy(gameObject);
               
            }
        }
    }
}
