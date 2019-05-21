using UnityEngine;

public class OilItem : MonoBehaviour {
    [SerializeField] bool consumed = false;
    [SerializeField] float amount = 100.0f;

    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false && consumed == false && player.InFeverGaugeNotEmpty == false) {
            player.RefillOil(amount);
            consumed = true;
            Destroy(gameObject);
        }
    }
}
