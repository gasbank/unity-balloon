using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindRegion : MonoBehaviour {
    [SerializeField] float windPower = 1.0f;
    [SerializeField] float maxCapacity = 0.0f;
    [SerializeField] float capacity = 0.0f;
    [SerializeField] float dissipationRate = 10.0f;
    [SerializeField] float rechargeRate = 10.0f;
    [SerializeField] Transform gaugeRed = null;
    [SerializeField] Transform gaugeGreen = null;
    [SerializeField] int enteredCount = 0;

    public Vector3 WindForce => windPower * transform.right;

    void OnTriggerEnter(Collider collider) {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            enteredCount++;
            if (maxCapacity == 0 || capacity > 0) {
                player.AddWindForce(this);
            }
        }
    }

    void OnTriggerStay(Collider collider) {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            if (maxCapacity != 0) {
                capacity = Mathf.Clamp(capacity - dissipationRate * Time.fixedDeltaTime, 0, maxCapacity);
                if (capacity <= 0) {
                    player.RemoveWindForce(this);
                }
            }
        }
    }

    void OnTriggerExit(Collider collider) {
        var player = collider.gameObject.GetComponentInParent<HotairBalloon>();
        if (player != null && player.IsGameOver == false) {
            enteredCount--;
            player.RemoveWindForce(this);
        }
    }

    void Update() {
        if (gaugeRed != null) {
            gaugeRed.gameObject.SetActive(maxCapacity != 0);
        }
        if (gaugeGreen != null) {
            gaugeGreen.gameObject.SetActive(maxCapacity != 0);
        }

        if (maxCapacity != 0) {
            if (enteredCount == 0) {
                capacity = Mathf.Clamp(capacity + rechargeRate * Time.deltaTime, 0, maxCapacity);
            }

            var ratio = Mathf.Clamp(capacity / maxCapacity, 0, 1);
            gaugeGreen.localScale = new Vector3(ratio * gaugeRed.localScale.x, gaugeGreen.localScale.y, gaugeGreen.localScale.z);
        }
    }
}
