using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;
using System.Linq;
using UnityEngine.SceneManagement;

public class HotairBalloon : MonoBehaviour {
    [SerializeField] Rigidbody balloonRb = null;
    [SerializeField] float defaultVelocity = 5.0f;
    [SerializeField] float maxDeg = 15.0f;
    [SerializeField] Transform oil = null;
    [SerializeField] float burnSpeed = 5.0f;
    [SerializeField] [Range(0, 100)] float remainOilAmount = 100.0f;
    [SerializeField] Transform balloon = null;
    [SerializeField] HashSet<WindRegion> appliedWindRegionSet = new HashSet<WindRegion>();

    internal void AddWindForce(WindRegion windRegion) {
        appliedWindRegionSet.Add(windRegion);
    }

    [SerializeField] Canvas gameOverGroup = null;
    [SerializeField] ParticleSystem[] fireParticleSystemList = null;

    internal void RemoveWindForce(WindRegion windRegion) {
        appliedWindRegionSet.Remove(windRegion);
    }

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
    [SerializeField] float boostRefillMaxInterval = 0.65f;
    [SerializeField] float boostInitialVelocity = 15.0f;
    [SerializeField] int boostRepeatCounter = 2;
    [SerializeField] TMProText stageStatText = null;
    [SerializeField] Transform directionArrowPivot = null;
    [SerializeField] float freeOilOnStartDuration = 5.0f;
    [SerializeField] TrailRenderer boostTrailRenderer = null;
    [SerializeField] float feverRemainTime = 0;

    public float FeverRemainTime => feverRemainTime;

    public float RemainOilAmount {
        get => remainOilAmount;
        private set { remainOilAmount = Mathf.Clamp(value, 0, 100); }
    }

    public bool IsGameOver => gameOverGroup.enabled;

    public bool IsFreeOilOnStart => Time.timeSinceLevelLoad < freeOilOnStartDuration;

    public bool IsOilConsumed => IsFreeOilOnStart == false && IsStageFinished == false;

    public bool IsStageFinished => finishGroup != null && finishGroup.enabled;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            handleSlider = GameObject.Find("Canvas/Slider").GetComponent<BalloonHandleSlider>();
        }
    }

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
        var vNormalized = new Vector3(Mathf.Cos(dirRad), Mathf.Sin(dirRad), 0);
        directionArrowPivot.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * dirRad);
        //v += Vector3.up * Input.GetAxis("Vertical") / 2;

        var emissionLeft = thrusterLeft.emission;
        var emissionRight = thrusterRight.emission;

        if (balloonRb.velocity.y < 0) {
            BalloonSound.instance.SetEngineVolume(0);
        } else if (balloonRb.velocity.y > 0) {
            BalloonSound.instance.SetEngineVolume(1);
        }

        if (IsStageFinished) {
            // 스테이지 완료 한 이후에는 그냥 위로만 쭈욱 올라가자
            emissionLeft.rateOverTime = 25;
            emissionRight.rateOverTime = 25;
            PlayTopThrusterPaticle();
            balloonRb.velocity = new Vector3(balloonRb.velocity.x, defaultVelocity, balloonRb.velocity.z);
            balloonRb.velocity += Vector3.up * boostVelocity;
        } else if (RemainOilAmount > 0) {
            // 연료가 남아있는 경우

            // 방향 조작을 하면 상승 + 좌우 이동
            if (horizontalAxis != 0) {
                balloonRb.velocity = defaultVelocity * vNormalized;
                balloonRb.velocity += Vector3.up * boostVelocity;

                if (vNormalized.x > 0.01f) {
                    emissionLeft.rateOverTime = 50;
                    emissionRight.rateOverTime = 0;
                } else if (vNormalized.x < -0.01f) {
                    emissionLeft.rateOverTime = 0;
                    emissionRight.rateOverTime = 50;
                } else {
                    emissionLeft.rateOverTime = 0;
                    emissionRight.rateOverTime = 0;
                }

                if (IsOilConsumed) {
                    RemainOilAmount -= Time.deltaTime * burnSpeed;
                }
            } else if (handleSlider.Controlled) {
                balloonRb.velocity = new Vector3(balloonRb.velocity.x, defaultVelocity, balloonRb.velocity.z);
                balloonRb.velocity += Vector3.up * boostVelocity;

                emissionLeft.rateOverTime = 25;
                emissionRight.rateOverTime = 25;

                if (IsOilConsumed) {
                    RemainOilAmount -= Time.deltaTime * burnSpeed;
                }
            } else if (IsFreeOilOnStart) {
                // 스테이지 시작하고 5초동안은 공짜로 위로 올라간다.
                balloonRb.velocity = new Vector3(balloonRb.velocity.x, defaultVelocity, balloonRb.velocity.z);
                balloonRb.velocity += Vector3.up * boostVelocity;

                emissionLeft.rateOverTime = 25;
                emissionRight.rateOverTime = 25;
            } else {
                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 0;
            }
        } else {
            // 추락 중에는 조타만 가능하게 한다.
            balloonRb.velocity = new Vector3(defaultVelocity * vNormalized.x, balloonRb.velocity.y, balloonRb.velocity.z);

            emissionLeft.rateOverTime = 0;
            emissionRight.rateOverTime = 0;
        }

        if (RemainOilAmount <= 0) {
            zeroOilDuration += Time.deltaTime;
            StopTopThrusterParticle();
        } else {
            zeroOilDuration = 0;
            if (horizontalAxis != 0 || IsFreeOilOnStart) {
                PlayTopThrusterPaticle();
            } else {
                if (IsStageFinished == false) {
                    StopTopThrusterParticle();
                }
            }
        }

        foreach (var windRegion in appliedWindRegionSet) {
            balloonRb.velocity += windRegion.WindForce;
        }

        if ((zeroOilDuration > 5.0f || balloon.position.y < -5) && gameOverGroup.enabled == false && IsStageFinished == false) {
            gameOverGroup.enabled = true;
            BalloonSound.instance.PlayGameOver();
            BalloonSound.instance.PlayGameOver_sigh();
        }

        float boostVelocityVelocity = 0;
        //boostTrailRenderer.gameObject.SetActive(boostVelocity > 5.0f);
        boostTrailRenderer.emitting = boostVelocity > 2.0f;
        boostVelocity = Mathf.SmoothDamp(boostVelocity, 0, ref boostVelocityVelocity, boostVelocityDamp);

        if (stageStatText != null) {
            stageStatText.SetText(string.Format("SPEED: {0:f1}\nHEIGHT: {1:f1}", balloonRb.velocity.magnitude, balloon.transform.position.y));
        }

        if (feverRemainTime > 0) {
            feverRemainTime -= Time.deltaTime;
            if (feverRemainTime <= 0) {
                StopFever();
            }
        }
    }

    bool engineRunning = false;

    private void PlayTopThrusterPaticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isPlaying == false) {
                engineRunning = true;
                if (IsGameOver == false && IsStageFinished == false) {
                    BalloonSound.instance.PlayStartEngine();
                }
                ps.Play();
            }
        }
    }

    private void StopTopThrusterParticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isStopped == false) {
                if (engineRunning && IsGameOver == false && IsStageFinished == false) {
                    BalloonSound.instance.PlayStopEngine();
                    engineRunning = false;
                }
                ps.Stop();
            }
        }
    }

    public void RefillOil(float amount) {
        Debug.Log("RefillOil");
        BalloonSound.instance.PlayGetOilItem();
        if (RemainOilAmount + amount > 100.0f && feverRemainTime <= 0) {
            StartFever();
        }
        RemainOilAmount = Mathf.Clamp(RemainOilAmount + amount, 0, 100.0f);
        if (Time.time - lastRefillTime < boostRefillMaxInterval) {
            Debug.Log("Boost Counter!");
            fastRefillCounter++;
            if (fastRefillCounter >= boostRepeatCounter) {
                boostVelocity = boostInitialVelocity;
                Debug.Log("Boost!!!");
                BalloonSound.instance.PlayStartBoost();
            }
        } else {
            fastRefillCounter = 0;
        }
        lastRefillTime = Time.time;
    }

    private void StartFever() {
        Debug.Log("Fever!!!");
        // var stageName = SceneManager.GetActiveScene().name;
        // var feverTouchedLayer = LayerMask.NameToLayer("Fever Touched");
        // foreach (var stageRb in GameObject.Find(stageName).GetComponentsInChildren<Rigidbody>()) {
        //     if (stageRb.gameObject.layer != feverTouchedLayer) {
        //         stageRb.isKinematic = false;
        //         stageRb.useGravity = false;
        //     }
        // }
        feverRemainTime = 10.0f;
    }

    private static void StopFever() {
        // var stageName = SceneManager.GetActiveScene().name;
        // var feverTouchedLayer = LayerMask.NameToLayer("Fever Touched");
        // foreach (var stageRb in GameObject.Find(stageName).GetComponentsInChildren<Rigidbody>()) {
        //     if (stageRb.gameObject.layer != feverTouchedLayer) {
        //         stageRb.isKinematic = true;
        //         stageRb.useGravity = true;
        //     }
        // }
    }

    internal void AddExplosionForce(Vector3 direction) {
        Debug.Log("AddExplosionForce");
        BalloonSound.instance.PlayKnockback();
        balloonRb.AddForce(direction * 10000);
    }
}
