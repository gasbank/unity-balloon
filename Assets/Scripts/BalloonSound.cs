using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonSound : MonoBehaviour {
    public static BalloonSound instance;

    // One-shot
    [SerializeField] AudioClip startEngine = null;
    [SerializeField] AudioClip getOilItem = null;
    [SerializeField] AudioClip stopEngine = null;
    [SerializeField] AudioClip gameOver = null;
    [SerializeField] AudioClip startBoost = null;
    [SerializeField] AudioClip knockback = null;
    [SerializeField] AudioClip goalincheer = null;
    [SerializeField] AudioClip goalinSound = null;
    [SerializeField] AudioClip gameover_sigh = null;
    [SerializeField] AudioClip dash_continuous = null;
    [SerializeField] AudioClip going_up = null;


    // Loop
    //[SerializeField] AudioClip ascendingLoop = null;
    //[SerializeField] AudioClip descendingLoop = null;

    [SerializeField] AudioSource ascendingLoopSource = null;
    [SerializeField] AudioSource descendingLoopSource = null;
    [SerializeField] AudioSource oneShotSource = null;

    public void PlayStartEngine() { Debug.Log("PlayStartEngine"); oneShotSource.PlayOneShot(startEngine); }
    public void PlayGetOilItem() { Debug.Log("PlayGetOilItem"); oneShotSource.PlayOneShot(getOilItem); }
    public void PlayStopEngine() { Debug.Log("PlayStopEngine"); oneShotSource.PlayOneShot(stopEngine); }
    public void PlayGameOver() { Debug.Log("PlayGameOver"); oneShotSource.PlayOneShot(gameOver); }
    public void PlayStartBoost() { Debug.Log("PlayStartBoost"); oneShotSource.PlayOneShot(startBoost); }
    public void PlayKnockback() { Debug.Log("PlayKnockback"); oneShotSource.PlayOneShot(knockback); }

    void Awake() {
        instance = this;
    }

    public void SetEngineVolume(float ratio) {
        ratio = Mathf.Clamp(ratio, 0, 1);
        ascendingLoopSource.volume = ratio;
        descendingLoopSource.volume = 1.0f - ratio;
    }
}
