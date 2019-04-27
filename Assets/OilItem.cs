using UnityEngine;

public class OilItem : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            player.RemainOilAmount = 100.0f;
            Destroy(gameObject);
        }
    }
}
