using UnityEngine;

public class OilItem : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            player.RefillOil();
            Destroy(gameObject);
        }
    }
}
