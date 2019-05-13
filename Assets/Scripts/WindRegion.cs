using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindRegion : MonoBehaviour {
    [SerializeField] float windPower = 1.0f;

    public Vector3 WindForce => windPower * transform.right;

    void OnTriggerEnter(Collider collider) {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            player.AddWindForce(this);
        }
    }

    void OnTriggerExit(Collider collider) {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            player.RemoveWindForce(this);
        }
    }

    // void OnTriggerStay(Collider collider) {
    //     var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
    //     if (player != null && player.IsGameOver == false) {
    //         player 
    //         if (collider.attachedRigidbody != null) {
    //             Debug.Log("RIGHT!!!");
    //             collider.attachedRigidbody.velocity += Vector3.right * windPower;
    //         }
    //     }
    // }
}
