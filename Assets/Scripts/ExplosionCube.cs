using UnityEngine;

public class ExplosionCube : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        var player = collision.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            
            player.AddExplosionForce((collision.transform.position - collision.contacts[0].point).normalized);
             player.IncreaseFeverGauge();
        }
    }
}
