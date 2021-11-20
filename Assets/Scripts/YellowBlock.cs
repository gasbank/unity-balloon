using UnityEngine;
using TMProText = TMPro.TextMeshPro;

public class YellowBlock : MonoBehaviour
{
    static Material halfRingMaterialCopy;

    [SerializeField]
    GameObject afterDestroySpawn;

    [SerializeField]
    Transform afterDestroySpawnParent;

    [SerializeField]
    Material halfRingMaterialOriginal;

    [SerializeField]
    Renderer halfRingRenderer;

    [SerializeField]
    int hp = 3;

    [SerializeField]
    TMProText hpText;

    [SerializeField]
    float lastHitMinInterval = 0.5f;

    [SerializeField]
    float lastHitTime;

    void Awake()
    {
        hpText.text = hp.ToString();
        if (halfRingMaterialOriginal != null)
            if (halfRingMaterialCopy == null)
                halfRingMaterialCopy = Instantiate(halfRingMaterialOriginal);
        if (halfRingRenderer != null) halfRingRenderer.material = halfRingMaterialCopy;
    }

    void Update()
    {
        if (halfRingMaterialCopy != null) halfRingMaterialCopy.mainTextureOffset = new Vector2(0, Time.time);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"YellowBlock collision enter: relativeVelocity magnitude: {collision.relativeVelocity.magnitude}");
        var player = collision.gameObject.GetComponentInParent<HotairBalloon>();
        if (Time.time - lastHitTime > lastHitMinInterval && player != null && player.IsGameOver == false)
        {
            lastHitTime = Time.time;
            hp--;
            hpText.text = hp.ToString();
            player.AddExplosionForce((collision.transform.position - collision.contacts[0].point).normalized);
            player.IncreaseFeverGauge(13);
            if (hp <= 0)
            {
                if (afterDestroySpawn != null)
                {
                    if (afterDestroySpawnParent != null)
                        Instantiate(afterDestroySpawn, afterDestroySpawnParent.position,
                            afterDestroySpawnParent.rotation);
                    else
                        Instantiate(afterDestroySpawn, transform.position, transform.rotation);
                }

                Destroy(gameObject);
            }
        }
    }
}