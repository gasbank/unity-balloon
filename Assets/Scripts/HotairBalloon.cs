using System;
using UnityEngine;

public class HotairBalloon : MonoBehaviour {
    [SerializeField] Rigidbody balloonRb = null;
    [SerializeField] float defaultVelocity = 5.0f;
    [SerializeField] float maxDeg = 15.0f;
    [SerializeField] Transform oil = null;
    [SerializeField] float burnSpeed = 5.0f;

    internal void AddExplosionForce(Vector3 direction) {
        Debug.Log("AddExplosionForce");
        balloonRb.AddForce(direction * 10000);
    }

    [SerializeField] [Range(0, 100)] float remainOilAmount = 100.0f;
    [SerializeField] Transform balloon = null;
    [SerializeField] GameObject gameOverGroup = null;
    [SerializeField] ParticleSystem[] fireParticleSystemList = null;
    [SerializeField] float zeroOilDuration = 0;
    [SerializeField] BalloonHandleSlider handleSlider = null;

    public float RemainOilAmount {
        get => remainOilAmount;
        private set { remainOilAmount = Mathf.Clamp(value, 0, 100); }
    }

    public bool IsGameOver => gameOverGroup.activeSelf;

    void Update() {
        oil.localScale = new Vector3(oil.localScale.x, remainOilAmount / 100.0f, oil.localScale.z);
        var horizontalAxis = Input.GetAxis("Horizontal");
        if (handleSlider != null) {
            horizontalAxis += handleSlider.Horizontal;
        }
        var v = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (90 - maxDeg * horizontalAxis)), Mathf.Sin(Mathf.Deg2Rad * (90 - maxDeg * horizontalAxis)), 0);
        v += Vector3.up * Input.GetAxis("Vertical") / 2;
        if (RemainOilAmount > 0) {
            balloonRb.velocity = defaultVelocity * v;
            RemainOilAmount -= Time.deltaTime * burnSpeed;
        }

        if (RemainOilAmount <= 0) {
            zeroOilDuration += Time.deltaTime;
            foreach (var ps in fireParticleSystemList) {
                if (ps.isPlaying) {
                    ps.Stop();
                }
            }
        } else {
            zeroOilDuration = 0;
            foreach (var ps in fireParticleSystemList) {
                if (ps.isPlaying == false) {
                    ps.Play();
                }
            }
        }

        if ((zeroOilDuration > 5.0f || balloon.position.y < -5) && gameOverGroup.activeSelf == false) {
            gameOverGroup.SetActive(true);
        }
    }

    public void RefillOil() {
        RemainOilAmount = 100.0f;
    }
}
