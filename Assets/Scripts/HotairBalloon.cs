using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

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
    [SerializeField] Transform balloonOilSpritePivot = null;
    [SerializeField] GameObject finishGroup = null;
    [SerializeField] int fastRefillCounter;
    [SerializeField] float lastRefillTime = 0;
    [SerializeField] float boostVelocity = 0;
    [SerializeField] float boostVelocityDamp = 0.3f;
    [SerializeField] float boostRefillMaxInterval = 0.5f;
    [SerializeField] float boostInitialVelocity = 15.0f;
    [SerializeField] int boostRepeatCounter = 2;
    [SerializeField] TMProText stageStatText = null;

    public float RemainOilAmount {
        get => remainOilAmount;
        private set { remainOilAmount = Mathf.Clamp(value, 0, 100); }
    }

    public bool IsGameOver => gameOverGroup.activeSelf;

    void Update() {
        oil.localScale = new Vector3(oil.localScale.x, remainOilAmount / 100.0f, oil.localScale.z);
        balloonOilSpritePivot.localPosition = new Vector3(balloonOilSpritePivot.localPosition.x, -0.1f + 0.2f * remainOilAmount / 100.0f, balloonOilSpritePivot.localPosition.z);

        var horizontalAxis = Input.GetAxis("Horizontal");
        if (handleSlider != null) {
            horizontalAxis += handleSlider.Horizontal;
        }
        var v = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (90 - maxDeg * horizontalAxis)), Mathf.Sin(Mathf.Deg2Rad * (90 - maxDeg * horizontalAxis)), 0);
        v += Vector3.up * Input.GetAxis("Vertical") / 2;
        if (RemainOilAmount > 0) {
            balloonRb.velocity = defaultVelocity * v + Vector3.up * boostVelocity;
            if (finishGroup == null || finishGroup.activeSelf == false) {
                RemainOilAmount -= Time.deltaTime * burnSpeed;
            }
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

        float boostVelocityVelocity = 0;
        boostVelocity = Mathf.SmoothDamp(boostVelocity, 0, ref boostVelocityVelocity, boostVelocityDamp);

        if (stageStatText != null) {
            stageStatText.SetText(string.Format("SPEED: {0:f1}\nHEIGHT: {1:f1}", balloonRb.velocity.magnitude, balloon.transform.position.y));
        }
    }

    public void RefillOil() {
        RemainOilAmount = 100.0f;
        if (Time.time - lastRefillTime < boostRefillMaxInterval) {
            Debug.Log("Boost Counter!");
            fastRefillCounter++;
            if (fastRefillCounter >= boostRepeatCounter) {
                boostVelocity = boostInitialVelocity;
                Debug.Log("Boost!!!");
            }
        } else {
            fastRefillCounter = 0;
        }
        lastRefillTime = Time.time;
    }
}
