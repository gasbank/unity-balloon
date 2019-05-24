using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonSound : MonoBehaviour {
    public static BalloonSound instance;

    // One-shot
    [SerializeField] AudioClip startEngine = null;
    [SerializeField] AudioClip getOilItem = null;
    [SerializeField] AudioClip gameOver = null;
    [SerializeField] AudioClip startBoost = null;
    [SerializeField] AudioClip knockback = null;
    [SerializeField] AudioClip feverstart = null;
    [SerializeField] AudioClip goalincheer = null;
    [SerializeField] AudioClip goalinSound = null;
    [SerializeField] AudioClip gameover_sigh = null;
#pragma warning disable CS0414
    [SerializeField] AudioClip dash_continuous = null;
    [SerializeField] AudioClip going_up = null;
#pragma warning restore CS0414
    [SerializeField] AudioClip maydaymayday = null;

    [SerializeField] AudioSource ascendingLoopSource = null;
    [SerializeField] AudioSource descendingLoopSource = null;
    [SerializeField] AudioSource oneShotSource = null;
    [SerializeField] AudioSource oneShotSource15 = null;
    [SerializeField] AudioSource oneShotSource20 = null;
    [SerializeField] AudioSource AmbientSound1 = null;
    
    public void PlayStartEngine() { Debug.Log("PlayStartEngine"); oneShotSource.PlayOneShot(startEngine); }
    public void PlayGetOilItem() { Debug.Log("PlayGetOilItem"); oneShotSource.PlayOneShot(getOilItem); }
    public void PlayGetOilItem2() { Debug.Log("PlayGetOilItem2"); oneShotSource15.PlayOneShot(getOilItem); }
    public void PlayGetOilItem3() { Debug.Log("PlayGetOilItem3"); oneShotSource20.PlayOneShot(getOilItem); }
    public void PlayGoalIn() { Debug.Log("GoalinSoubnd"); oneShotSource.PlayOneShot(goalinSound); }
    public void PlayCheer() { Debug.Log("GoalinCheer"); oneShotSource.PlayOneShot(goalincheer); }
    public void PlayGameOver() { Debug.Log("PlayGameOver"); oneShotSource.PlayOneShot(gameOver); }
    public void PlayGameOver_sigh() { Debug.Log("PlayGameOver"); oneShotSource.PlayOneShot(gameover_sigh); }
    public void PlayStartBoost() { Debug.Log("PlayStartBoost"); oneShotSource.PlayOneShot(startBoost); }
    public void PlayKnockback() { Debug.Log("PlayKnockback"); oneShotSource.PlayOneShot(knockback); }
    public void PlayFever() { Debug.Log("PlayFever"); oneShotSource.PlayOneShot(feverstart); }
    public void PlayMaydayMayday() { Debug.Log("MaydayMayday"); oneShotSource.PlayOneShot(maydaymayday); }
    
    void Awake() {
        instance = this;
    }

    public void SetEngineVolume(float ratio) {
        ratio = Mathf.Clamp(ratio, 0, 1);
        ascendingLoopSource.volume = ratio;
        descendingLoopSource.volume = 1.0f - ratio;
        AmbientSound1.volume = 0.5f;
    }
}
