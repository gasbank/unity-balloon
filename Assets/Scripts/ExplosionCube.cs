using UnityEngine;

public class ExplosionCube : MonoBehaviour
{
    [SerializeField]
    Transform fuelPivot;

    [SerializeField]
    ParticleSystem onRechargedEffect;

    [SerializeField]
    bool rechargeable;

    [SerializeField]
    float rechargeGauge = 2.0f;

    [SerializeField]
    float rechargeGaugeMax = 2.0f;

    [SerializeField]
    float rechargeSpeed = 2.0f;

    public bool CanExplode => rechargeable == false || rechargeGauge >= rechargeGaugeMax;

    void Update()
    {
        if (rechargeable)
        {
            var oldCanExplode = CanExplode;
            rechargeGauge = Mathf.Clamp(rechargeGauge + rechargeSpeed * Time.deltaTime, 0, rechargeGaugeMax);
            if (onRechargedEffect != null && oldCanExplode == false && CanExplode) onRechargedEffect.Play();
            if (fuelPivot != null) fuelPivot.localScale = Vector3.one * rechargeGauge / rechargeGaugeMax;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false && CanExplode)
        {
            player.AddExplosionForce((collision.transform.position - collision.contacts[0].point).normalized);
            player.IncreaseFeverGauge(8);
            rechargeGauge = 0;
        }
    }
}