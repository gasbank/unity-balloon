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
    [SerializeField] Canvas gameOverGroup = null;
    [SerializeField] ParticleSystem[] fireParticleSystemList = null;
    [SerializeField] ParticleSystem thrusterLeft = null;
    [SerializeField] ParticleSystem thrusterRight = null;
    [SerializeField] float zeroOilDuration = 0;
    [SerializeField] BalloonHandleSlider handleSlider = null;
    [SerializeField] Transform balloonOilSpritePivot = null;
    [SerializeField] Canvas finishGroup = null;
    [SerializeField] int fastRefillCounter;
    [SerializeField] float lastRefillTime = 0;
    [SerializeField] float boostVelocity = 0;
    [SerializeField] float boostVelocityDamp = 0.3f;
    [SerializeField] float boostRefillMaxInterval = 0.5f;
    [SerializeField] float boostInitialVelocity = 15.0f;
    [SerializeField] int boostRepeatCounter = 2;
    [SerializeField] TMProText stageStatText = null;
    [SerializeField] Transform directionArrowPivot = null;


    public float RemainOilAmount {
        get => remainOilAmount;
        private set { remainOilAmount = Mathf.Clamp(value, 0, 100); }
    }

    public bool IsGameOver => gameOverGroup.enabled;

    void Awake() {
        gameOverGroup = FindObjectOfType<GameOverGroup>().GetComponent<Canvas>();
        finishGroup = FindObjectOfType<FinishGroup>().GetComponent<Canvas>();
    }

    void Update() {
        oil.localScale = new Vector3(oil.localScale.x, remainOilAmount / 100.0f, oil.localScale.z);
        balloonOilSpritePivot.localPosition = new Vector3(balloonOilSpritePivot.localPosition.x, -0.1f + 0.2f * remainOilAmount / 100.0f, balloonOilSpritePivot.localPosition.z);

        var horizontalAxis = Input.GetAxis("Horizontal");
        if (handleSlider != null) {
            horizontalAxis += handleSlider.Horizontal;
        }

        var dirRad = Mathf.Deg2Rad * (90 - maxDeg * horizontalAxis);
        var v = new Vector3(Mathf.Cos(dirRad), Mathf.Sin(dirRad), 0);
        directionArrowPivot.rotation = Quaternion.Euler(0, 0, - 90 + Mathf.Rad2Deg * dirRad);
        v += Vector3.up * Input.GetAxis("Vertical") / 2;

        var emissionLeft = thrusterLeft.emission;
        var emissionRight = thrusterRight.emission;

        if (RemainOilAmount > 0 && horizontalAxis != 0) {
            balloonRb.velocity = defaultVelocity * v + Vector3.up * boostVelocity;

            if (v.x > 0.01f) {
                emissionLeft.rateOverTime = 50;
                emissionRight.rateOverTime = 0;
            } else if (v.x < -0.01f) {
                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 50;
            } else {
                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 0;
            }

            if (finishGroup == null || finishGroup.enabled == false) {
                RemainOilAmount -= Time.deltaTime * burnSpeed;
            }
        } else {
            // 추락 중에는 조타만 가능하게 한다.
            balloonRb.velocity = new Vector3(defaultVelocity * v.x, balloonRb.velocity.y, balloonRb.velocity.z);

            emissionLeft.rateOverTime = 0;
            emissionRight.rateOverTime = 0;
        }

        if (RemainOilAmount <= 0) {
            zeroOilDuration += Time.deltaTime;
            StopTopThrusterParticle();
        } else {
            zeroOilDuration = 0;
            if (horizontalAxis != 0) {
                PlayTopThrusterPaticle();
            } else {
                StopTopThrusterParticle();
            }
        }

        if ((zeroOilDuration > 5.0f || balloon.position.y < -5) && gameOverGroup.enabled == false) {
            gameOverGroup.enabled = true;
        }

        float boostVelocityVelocity = 0;
        boostVelocity = Mathf.SmoothDamp(boostVelocity, 0, ref boostVelocityVelocity, boostVelocityDamp);

        if (stageStatText != null) {
            stageStatText.SetText(string.Format("SPEED: {0:f1}\nHEIGHT: {1:f1}", balloonRb.velocity.magnitude, balloon.transform.position.y));
        }
    }

    private void PlayTopThrusterPaticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isPlaying == false) {
                ps.Play();
            }
        }
    }

    private void StopTopThrusterParticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isPlaying) {
                ps.Stop();
            }
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
