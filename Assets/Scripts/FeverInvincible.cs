using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeverInvincible : MonoBehaviour {
    [SerializeField] HotairBalloon balloon = null;

    private void OnCollisionEnter(Collision collision) {
        if (balloon.InFeverGaugeNotEmpty && collision.gameObject != null) {
            //collision.rigidbody.useGravity = true;
            if (collision.rigidbody != null && collision.rigidbody.isKinematic) {
                collision.rigidbody.isKinematic = false;
                Destroy(collision.gameObject, 3.0f);
            }
        }
    }
}
