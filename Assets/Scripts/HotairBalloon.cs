using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

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
    [SerializeField] PostProcessVolume postProcessVolume = null;
    [SerializeField] Renderer feverRingRenderer = null;
    [SerializeField] float feverGaugeMax = 50.0f;
    [SerializeField] float feverGaugeIncrement = 8.0f;
    [SerializeField] float feverGaugeDecrement = 3.0f;
    [SerializeField] GameObject feverItemParticle = null;
    [SerializeField] bool inFever = false;
    [SerializeField] float feverMaxVelocity = 30;
    [SerializeField] bool verticallyStationary = false;
    [SerializeField] FixedJoint[] fixedJointArray = null;
    [SerializeField] Gear gear = null;

    Vignette vignette;

    public float RemainOilAmount {
        get => remainOilAmount;
        private set { remainOilAmount = Mathf.Clamp(value, 0, 100); }
    }

    public bool IsGameOver => gameOverGroup.enabled;

    public bool IsFreeOilOnStart => Time.timeSinceLevelLoad < freeOilOnStartDuration;

    public bool IsOilConsumed =>
        IsFreeOilOnStart == false
        && IsStageFinished == false
        && IsGameOver == false
        && InFeverGaugeNotEmpty == false
        && (HorizontalAxis != 0 || handleSlider.Controlled || InFeverGaugeNotEmpty);

    public bool IsStageFinished =>
        finishGroup != null
        && finishGroup.enabled;

    float FeverGauge {
        get => (1.0f - feverRingRenderer.material.mainTextureOffset.y) * feverGaugeMax;
        set {
            var y = Mathf.Clamp(1.0f - (value / feverGaugeMax), 0, 1);
            feverRingRenderer.material.mainTextureOffset = new Vector2(feverRingRenderer.material.mainTextureOffset.x, y);
        }
    }

    [SerializeField] Rigidbody[] rbArray = null;
    [SerializeField] Collider[] colliderArray = null;

    public bool InFeverGaugeNotEmpty => inFever && FeverGauge > 0;

    public bool BalloonGameOverCondition => zeroOilDuration > 5.0f || balloon.position.y < -5;

    public void IncreaseFeverGauge() {
        if (inFever) {
            return;
        }

        if (FeverGauge < feverGaugeMax && FeverGauge + feverGaugeIncrement >= feverGaugeMax) {
            Debug.Log("Fever full!");
            feverItemParticle.SetActive(true);
        }
        FeverGauge += feverGaugeIncrement;
    }

    float AdditionalVelocity => boostVelocity + (InFeverGaugeNotEmpty ? feverMaxVelocity : 0);

    public float Y => balloonRb.position.y;

    public bool VerticallyStationary {
        get => verticallyStationary;
        set => verticallyStationary = value;
    }

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            handleSlider = GameObject.Find("Canvas/Slider").GetComponent<BalloonHandleSlider>();
        }
    }

    void Awake() {
        gameOverGroup = FindObjectOfType<GameOverGroup>().GetComponent<Canvas>();
        finishGroup = FindObjectOfType<FinishGroup>().GetComponent<Canvas>();

        var postProcessVolumeGo = GameObject.Find("Main Camera/Post Process Volume");
        if (postProcessVolume != null) {
            postProcessVolume = postProcessVolumeGo.GetComponent<PostProcessVolume>();
            if (postProcessVolume != null) {
                postProcessVolume.profile.TryGetSettings(out vignette);
            }
        }

        feverRingRenderer.material = Instantiate(feverRingRenderer.material);
        FeverGauge = 0;

        rbArray = GetComponentsInChildren<Rigidbody>();
        fixedJointArray = GetComponentsInChildren<FixedJoint>();
        colliderArray = GetComponentsInChildren<Collider>();
    }

    float HorizontalAxis => Input.GetAxis("Horizontal") + (handleSlider != null ? handleSlider.Horizontal : 0);

    void FixedUpdate() {
        var emissionLeft = thrusterLeft.emission;
        var emissionRight = thrusterRight.emission;
        var dirRad = Mathf.Deg2Rad * (90 - maxDeg * HorizontalAxis);
        var vNormalized = new Vector3(Mathf.Cos(dirRad), Mathf.Sin(dirRad), 0);
        
        if (IsGameOver) {
            StopTopThrusterParticle();
            emissionLeft.rateOverTime = 0;
            emissionRight.rateOverTime = 0;
        } else if (IsStageFinished) {
            // 스테이지 완료 한 이후에는 그냥 위로만 쭈욱 올라가자
            emissionLeft.rateOverTime = 25;
            emissionRight.rateOverTime = 25;
            PlayTopThrusterPaticle();
            balloonRb.velocity = new Vector3(balloonRb.velocity.x, defaultVelocity, balloonRb.velocity.z);
            balloonRb.velocity += Vector3.up * AdditionalVelocity;
        } else if (RemainOilAmount > 0 || InFeverGaugeNotEmpty) {
            // 연료가 남아있는 경우 또는 피버 중
            if (HorizontalAxis != 0) {
                // 방향 조작을 하고 있는 중이면 상승 + 좌우 이동
                balloonRb.velocity = defaultVelocity * vNormalized;
                balloonRb.velocity += Vector3.up * AdditionalVelocity;

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
            } else if (handleSlider.Controlled || InFeverGaugeNotEmpty) {
                // 터치만 하고 있는 상태(방향 조작 0)라면 위로만 올라가면 된다.
                balloonRb.velocity = new Vector3(balloonRb.velocity.x, defaultVelocity, balloonRb.velocity.z);
                balloonRb.velocity += Vector3.up * AdditionalVelocity;

                emissionLeft.rateOverTime = 25;
                emissionRight.rateOverTime = 25;

                if (IsOilConsumed) {
                    RemainOilAmount -= Time.deltaTime * burnSpeed;
                }
            } else if (VerticallyStationary) {
                PlayTopThrusterPaticle();
                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 0;
            } else if (IsFreeOilOnStart) {
                // 스테이지 시작하고 5초동안은 공짜로 위로 올라간다.
                balloonRb.velocity = new Vector3(balloonRb.velocity.x, defaultVelocity, balloonRb.velocity.z);
                balloonRb.velocity += Vector3.up * AdditionalVelocity;

                emissionLeft.rateOverTime = 25;
                emissionRight.rateOverTime = 25;
            } else {
                // 좌우 엔진 효과 끈다. 물리 법칙에 맡긴다. (자유 낙하)
                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 0;
            }
        } else if (IsStageFinished == false && IsGameOver == false) {
            // 추락 중에는 조타만 가능하게 한다.
            balloonRb.velocity = new Vector3(defaultVelocity * vNormalized.x, balloonRb.velocity.y, balloonRb.velocity.z);

            emissionLeft.rateOverTime = 0;
            emissionRight.rateOverTime = 0;
        }

        if (IsGameOver == false) {
            foreach (var windRegion in appliedWindRegionSet) {
                balloonRb.velocity += windRegion.WindForce;
            }
        }
    }

    void Update() {
        oil.localScale = new Vector3(oil.localScale.x, remainOilAmount / 100.0f, oil.localScale.z);
        balloonOilSpritePivot.localPosition = new Vector3(balloonOilSpritePivot.localPosition.x, -0.1f + 0.2f * remainOilAmount / 100.0f, balloonOilSpritePivot.localPosition.z);

        var dirRad = Mathf.Deg2Rad * (90 - maxDeg * HorizontalAxis);
        var vNormalized = new Vector3(Mathf.Cos(dirRad), Mathf.Sin(dirRad), 0);
        directionArrowPivot.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * dirRad);
        
        if (balloonRb.velocity.y < 0) {
            BalloonSound.instance.SetEngineVolume(0);
        } else if (balloonRb.velocity.y > 0) {
            BalloonSound.instance.SetEngineVolume(1);
        }

        // 기름이 바닥난 상태라면...
        if (RemainOilAmount <= 0) {
            // 연료가 바닥난 순간 한번만 처리해야 하는 일은 여기서 한다.
            if (zeroOilDuration == 0) {
                BalloonSound.instance.PlayMaydayMayday();
            }
            zeroOilDuration += Time.deltaTime;

            StopTopThrusterParticle();
        } else {
            zeroOilDuration = 0;
            if (HorizontalAxis != 0 || IsFreeOilOnStart) {
                PlayTopThrusterPaticle();
            } else {
                if (IsStageFinished == false) {
                    StopTopThrusterParticle();
                }
            }
        }

        // 연료 바닥났을 때 화면 효과
        if (vignette != null) {
            if (zeroOilDuration > 0) {
                vignette.intensity.value = (0.5f + Mathf.PingPong(Time.time * 0.7f, 0.1f));
            } else {
                vignette.intensity.value = 0;
            }
        }

        if (BalloonGameOverCondition
            && gameOverGroup.enabled == false
            && IsStageFinished == false) {
            gameOverGroup.enabled = true;
            BalloonSound.instance.PlayGameOver();
            BalloonSound.instance.PlayGameOver_sigh();
            foreach (var fixedJoint in fixedJointArray) {
                Destroy(fixedJoint);
            }
            foreach (var rb in rbArray) {
                rb.constraints = 0;
            }
            foreach (var collider in colliderArray) {
                collider.enabled = true;
            }
            gear.enabled = false;
        }

        float boostVelocityVelocity = 0;
        //boostTrailRenderer.gameObject.SetActive(boostVelocity > 5.0f);
        boostTrailRenderer.emitting = boostVelocity > 2.0f;
        boostVelocity = Mathf.SmoothDamp(boostVelocity, 0, ref boostVelocityVelocity, boostVelocityDamp);

        if (stageStatText != null) {
            stageStatText.SetText(string.Format("SPEED: {0:f1}\nHEIGHT: {1:f1}", balloonRb.velocity.magnitude, balloon.transform.position.y));
        }

        // AddForce라서 FixedUpdate()에 있는 게 일반적이지만,
        // 타입이 Impulse이니 Update()에 넣는다.
        if (VerticallyStationary && balloonRb.position.y < 0 && RemainOilAmount > 0) {
            balloonRb.AddForce(Vector3.up * (-5 * balloonRb.position.y - 2 * balloonRb.velocity.y), ForceMode.Impulse);
        }

        // 피버 아이템을 가지고 있지 않을 때만 감소
        if (feverItemParticle.activeSelf == false) {
            FeverGauge -= Time.deltaTime * 2 * feverGaugeDecrement;
            // 피버 게이지가 바닥 나면
            if (FeverGauge <= 0) {
                // 피버 모드 종료
                inFever = false;
            }
        }
    }

    bool engineRunning = false;

    private void PlayTopThrusterPaticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isPlaying == false) {
                engineRunning = true;
                if (IsGameOver == false && IsStageFinished == false) {
                    // VerticallyStationary에 의해서 Y=0 부근에서 진동 일어날 때
                    // 엔진 사운드가 너무 반복적인 문제를 해결하기 위한 조건문
                    if (Y > 1) {
                        BalloonSound.instance.PlayStartEngine();
                    }
                }
                ps.Play();
            }
        }
    }

    private void StopTopThrusterParticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isStopped == false) {
                if (engineRunning && IsGameOver == false && IsStageFinished == false) {
                    // VerticallyStationary에 의해서 Y=0 부근에서 진동 일어날 때
                    // 엔진 사운드가 너무 반복적인 문제를 해결하기 위한 조건문
                    if (Y > 1) {
                        BalloonSound.instance.PlayStopEngine();
                    }
                    engineRunning = false;
                }
                ps.Stop();
            }
        }
    }

    public void RefillOil(float amount) {
        Debug.Log("RefillOil");
        BalloonSound.instance.PlayGetOilItem();
        // if (RemainOilAmount + amount > 100.0f && feverRemainTime <= 0) {
        //     StartFever();
        // }
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

    public void StartFever() {
        if (IsGameOver == false && feverItemParticle.activeSelf) {
            feverItemParticle.SetActive(false);
            inFever = true;
            Debug.Log("Fever!!!");
        }
    }

    private static void StopFever() {

    }

    internal void AddExplosionForce(Vector3 direction) {
        Debug.Log("AddExplosionForce");
        BalloonSound.instance.PlayKnockback();
        balloonRb.AddForce(direction * 10000);
    }
}
