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

    [SerializeField] GameOverGroup gameOverGroup = null;
    [SerializeField] ParticleSystem[] fireParticleSystemList = null;

    internal void RemoveWindForce(WindRegion windRegion) {
        appliedWindRegionSet.Remove(windRegion);
    }

    [SerializeField] ParticleSystem thrusterLeft = null;
    [SerializeField] ParticleSystem thrusterRight = null;
    [SerializeField] float zeroOilDuration = 0;
    [SerializeField] BalloonHandleSlider handleSlider = null;
    [SerializeField] Transform balloonOilSpritePivot = null;
    [SerializeField] FinishGroup finishGroup = null;
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
    [SerializeField] Transform balloonFeverRing = null;
    [SerializeField] Transform balloonFeverRingOuter = null;
    [SerializeField] Renderer feverRingRenderer = null;
    [SerializeField] float feverGaugeMax = 50.0f;
    [SerializeField] float feverGaugeIncrement = 8.0f;
    [SerializeField] float feverGaugeDecrementByDecay = 8.0f;
    [SerializeField] float feverGaugeDecrementByConsumption = 15.0f;
    [SerializeField] GameObject feverItemParticle = null;
    [SerializeField] bool inFever = false;
    [SerializeField] float feverMaxVelocity = 30;
    [SerializeField] bool verticallyStationary = false;
    [SerializeField] FixedJoint[] fixedJointArray = null;
    [SerializeField] Gear gear = null;
    [SerializeField] ParticleSystem feverStart = null;
    [SerializeField] ParticleSystem feverThrust = null;
    [SerializeField] StageCommon stageCommon = null;
    [SerializeField] float stageElapsedTime = 0;

    Vignette vignette;

    public float StageElapsedTime => stageElapsedTime;

    public float RemainOilAmount {
        get => remainOilAmount;
        private set { remainOilAmount = Mathf.Clamp(value, 0, 100); }
    }

    public float RemainOilAmountRatio => RemainOilAmount / 100;

    public bool IsGameOver => gameOverGroup.Visible;

    public bool IsFreeOilOnStart => StageElapsedTime < freeOilOnStartDuration;

    public bool IsOilConsumed =>
        IsFreeOilOnStart == false
        && IsStageFinished == false
        && IsGameOver == false
        && InFeverGaugeNotEmpty == false
        && (HorizontalAxis != 0 || handleSlider.Controlled || InFeverGaugeNotEmpty);

    public bool IsStageFinished {
        get => finishGroup.Visible;
        set => finishGroup.Visible = value;
    }

    float FeverGauge {
        get {
            if (Application.isPlaying) {
                return (1.0f - feverRingRenderer.material.mainTextureOffset.y) * feverGaugeMax;
            } else {
                return 1.0f;
            }
        }
        set {
            var y = Mathf.Clamp(1.0f - (value / feverGaugeMax), 0, 1);
            feverRingRenderer.material.mainTextureOffset = new Vector2(feverRingRenderer.material.mainTextureOffset.x, y);
        }
    }

    public float FeverGaugeRatio => FeverGauge / feverGaugeMax;

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

    public bool CanStartFever => feverItemParticle.activeSelf;

    public bool VerticallyStationary {
        get => verticallyStationary;
        set => verticallyStationary = value;
    }

    public bool IsVerticallyStationaryForceApplied => (IsTitleVisible || VerticallyStationary) && balloonRb.position.y < 0 && RemainOilAmount > 0;

    public bool IsTitleVisible => stageCommon.IsTitleVisible;

    public Vector3 FeverRingPosition => balloonFeverRing.position;

    public Vector3 FeverRingOuterPosition => balloonFeverRingOuter.position;

    void OnValidate() {
        if (gameObject.scene.rootCount != 0) {
            handleSlider = GameObject.Find("Canvas/Slider").GetComponent<BalloonHandleSlider>();
        }
    }

    void Awake() {
        Application.runInBackground = false;
        if (Application.isMobilePlatform == false) {
            Screen.SetResolution(720, 1280, FullScreenMode.Windowed);
        }
        gameOverGroup = FindObjectOfType<GameOverGroup>();
        finishGroup = FindObjectOfType<FinishGroup>();

        var postProcessVolumeGo = GameObject.Find("Main Camera/Post Process Volume");
        if (postProcessVolumeGo != null) {
            postProcessVolume = postProcessVolumeGo.GetComponent<PostProcessVolume>();
            if (postProcessVolume != null) {
                postProcessVolume.profile.TryGetSettings(out vignette);
            }
        }
        if (vignette == null) {
            Debug.LogError("Vignette cannot be found.");
        }

        feverRingRenderer.material = Instantiate(feverRingRenderer.material);
        FeverGauge = 0;

        rbArray = GetComponentsInChildren<Rigidbody>();
        fixedJointArray = GetComponentsInChildren<FixedJoint>();
        colliderArray = GetComponentsInChildren<Collider>();
        stageCommon = GameObject.FindObjectOfType<StageCommon>();
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
            PlayTopThrusterParticle();
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

                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 0;

                if (IsOilConsumed) {
                    RemainOilAmount -= Time.deltaTime * burnSpeed;
                }
            } else if (IsVerticallyStationaryForceApplied) {
                PlayTopThrusterParticle();
                emissionLeft.rateOverTime = 0;
                emissionRight.rateOverTime = 0;
            } else if (IsFreeOilOnStart && IsTitleVisible == false) {
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
        if (HorizontalAxis != 0) {
            handleSlider.OnStartStage();
        }

        if (IsGameOver == false && IsStageFinished == false && IsTitleVisible == false) {
            stageElapsedTime += Time.deltaTime;
        }

        oil.localScale = new Vector3(oil.localScale.x, RemainOilAmount / 100.0f, oil.localScale.z);
        balloonOilSpritePivot.localPosition = new Vector3(balloonOilSpritePivot.localPosition.x, -0.1f + 0.2f * RemainOilAmount / 100.0f, balloonOilSpritePivot.localPosition.z);

        var dirRad = Mathf.Deg2Rad * (90 - maxDeg * HorizontalAxis);
        var vNormalized = new Vector3(Mathf.Cos(dirRad), Mathf.Sin(dirRad), 0);
        directionArrowPivot.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * dirRad);
        
        if (IsVerticallyStationaryForceApplied) {
            BalloonSound.instance.SetEngineVolume(1);
        } else if (balloonRb.velocity.y < 0) {
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
            if (handleSlider.Controlled || IsFreeOilOnStart) {
                PlayTopThrusterParticle();
            } else {
                if (IsStageFinished == false && IsVerticallyStationaryForceApplied == false && Y > 2) {
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
            && gameOverGroup.Visible == false
            && IsStageFinished == false) {
            gameOverGroup.Visible = true;
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
        if (IsVerticallyStationaryForceApplied) {
            balloonRb.AddForce(Vector3.up * (-5 * balloonRb.position.y - 2 * balloonRb.velocity.y), ForceMode.Impulse);
        }

        // 피버 아이템을 가지고 있지 않을 때만 감소
        if (CanStartFever == false) {
            if (inFever) {
                FeverGauge -= Time.deltaTime * feverGaugeDecrementByConsumption;
            } else {
                FeverGauge -= Time.deltaTime * feverGaugeDecrementByDecay;
            }
            // 피버 게이지가 바닥 나면
            if (FeverGauge <= 0) {
                // 피버 모드 종료
                inFever = false;
                feverThrust.Stop();
            }
        }
    }

    bool engineRunning = false;
    private float lastPlayStartEngineSoundTime;

    private void PlayTopThrusterParticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isPlaying == false) {
                engineRunning = true;
                if (IsGameOver == false && IsStageFinished == false) {
                    // VerticallyStationary에 의해서 Y=0 부근에서 진동 일어날 때
                    // 엔진 사운드가 너무 반복적인 문제를 해결하기 위한 조건문
                    if (IsVerticallyStationaryForceApplied == false) {
                        if (Time.time - lastPlayStartEngineSoundTime > 0.5f) {
                            lastPlayStartEngineSoundTime = Time.time;
                            BalloonSound.instance.PlayStartEngine();
                        }
                    }
                }
                ps.Play();
                Debug.Log("PlayTopThrusterParticle - Played");
            }
        }
    }

    private void StopTopThrusterParticle() {
        foreach (var ps in fireParticleSystemList) {
            if (ps != null && ps.isEmitting == true) {
                if (engineRunning && IsGameOver == false && IsStageFinished == false) {
                    engineRunning = false;
                }
                ps.Stop();
                Debug.Log("PlayTopThrusterParticle - Stopped");
            }
        }
    }

    public void RefillOil(float amount) {
        Debug.Log("RefillOil");
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

        // 얼마나 빠르게 연속으로 연료를 먹었냐에 따라 재생되는 사운드 피치가 다르게 하자.
        // (사운드 파일은 동일)
        if (fastRefillCounter <= 0) {
            BalloonSound.instance.PlayGetOilItem();
        } else if (fastRefillCounter <= 1) {
            BalloonSound.instance.PlayGetOilItem2();
        } else {
            BalloonSound.instance.PlayGetOilItem3();
        }

        lastRefillTime = Time.time;
    }

    public void StartFever() {
        if (IsGameOver == false && CanStartFever) {
            feverItemParticle.SetActive(false);
            inFever = true;
            BalloonSound.instance.PlayFever();
            RemainOilAmount = 100.0f;
            feverStart.Stop();
            feverStart.Play();
            feverThrust.Stop();
            feverThrust.Play();
            Debug.Log("Fever!!!");
        }
    }

    internal void AddExplosionForce(Vector3 direction) {
        Debug.Log("AddExplosionForce");
        BalloonSound.instance.PlayKnockback();
        balloonRb.AddForce(direction * 10000);
    }
}
