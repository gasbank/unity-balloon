using UnityEngine;

public class WindRegion : MonoBehaviour
{
    [SerializeField]
    float capacity;

    [SerializeField]
    float dissipationRate = 10.0f;

    [SerializeField]
    int enteredCount;

    [SerializeField]
    Transform gaugeGreen;

    [SerializeField]
    Transform gaugeRed;

    [SerializeField]
    float maxCapacity;

    [SerializeField]
    float rechargeRate = 10.0f;

    public Vector3 WindForce => transform != null ? Power * transform.right : Vector3.zero;

    [field: SerializeField]
    public float Power { get; } = 1.0f;

    public float CapacityRatio => capacity / maxCapacity;

    void OnTriggerEnter(Collider collider)
    {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false)
        {
            enteredCount++;
            if (maxCapacity == 0 || capacity > 0) player.AddWindForce(this);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false)
            if (maxCapacity != 0)
            {
                capacity = Mathf.Clamp(capacity - dissipationRate * Time.fixedDeltaTime, 0, maxCapacity);
                if (capacity <= 0) player.RemoveWindForce(this);
            }
    }

    void OnTriggerExit(Collider collider)
    {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false)
        {
            enteredCount--;
            player.RemoveWindForce(this);
        }
    }

    void Update()
    {
        if (gaugeRed != null) gaugeRed.gameObject.SetActive(maxCapacity != 0);
        if (gaugeGreen != null) gaugeGreen.gameObject.SetActive(maxCapacity != 0);

        if (maxCapacity != 0)
        {
            if (enteredCount == 0) capacity = Mathf.Clamp(capacity + rechargeRate * Time.deltaTime, 0, maxCapacity);

            var ratio = Mathf.Clamp(capacity / maxCapacity, 0, 1);
            gaugeGreen.localScale = new Vector3(ratio * gaugeRed.localScale.x, gaugeGreen.localScale.y,
                gaugeGreen.localScale.z);
        }
    }
}