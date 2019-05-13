using UnityEngine;

public class OilItem : MonoBehaviour {
    [SerializeField] bool consumed = false;

    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false && consumed == false) {
            player.RefillOil();
            consumed = true;
            Destroy(gameObject);
        }
    }
}
