using UnityEngine;
using UnityEngine.Audio;

public class BalloonSound : MonoBehaviour
{
    public static BalloonSound instance;

    [SerializeField]
    AudioSource AmbientSound1;

    [SerializeField]
    AudioSource ascendingLoopSource;

    [SerializeField]
    AudioMixer audioMixer;

    [SerializeField]
    AudioSource descendingLoopSource;

    [SerializeField]
    AudioClip feverstart;

    [SerializeField]
    AudioClip gameOver;

    [SerializeField]
    AudioClip gameover_sigh;

    [SerializeField]
    AudioClip getOilItem;

    [SerializeField]
    AudioClip goalincheer;

    [SerializeField]
    AudioClip goalinSound;

    [SerializeField]
    AudioClip knockback;

    [SerializeField]
    AudioClip maydaymayday;

    [SerializeField]
    AudioSource oneShotSource;

    [SerializeField]
    AudioSource oneShotSource15;

    [SerializeField]
    AudioSource oneShotSource20;

    [SerializeField]
    AudioClip startBoost;

    // One-shot
    [SerializeField]
    AudioClip startEngine;

    public void PlayStartEngine()
    {
        Debug.Log("PlayStartEngine");
        oneShotSource.PlayOneShot(startEngine);
    }

    public void PlayGetOilItem()
    {
        Debug.Log("PlayGetOilItem");
        oneShotSource.PlayOneShot(getOilItem);
    }

    public void PlayGetOilItem2()
    {
        Debug.Log("PlayGetOilItem2");
        oneShotSource15.PlayOneShot(getOilItem);
    }

    public void PlayGetOilItem3()
    {
        Debug.Log("PlayGetOilItem3");
        oneShotSource20.PlayOneShot(getOilItem);
    }

    public void PlayGoalIn()
    {
        Debug.Log("GoalinSoubnd");
        oneShotSource.PlayOneShot(goalinSound);
    }

    public void PlayCheer()
    {
        Debug.Log("GoalinCheer");
        oneShotSource.PlayOneShot(goalincheer);
    }

    public void PlayGameOver()
    {
        Debug.Log("PlayGameOver");
        oneShotSource.PlayOneShot(gameOver);
    }

    public void PlayGameOver_sigh()
    {
        Debug.Log("PlayGameOver");
        oneShotSource.PlayOneShot(gameover_sigh);
    }

    public void PlayStartBoost()
    {
        Debug.Log("PlayStartBoost");
        oneShotSource.PlayOneShot(startBoost);
    }

    public void PlayKnockback()
    {
        Debug.Log("PlayKnockback");
        oneShotSource.PlayOneShot(knockback);
    }

    public void PlayFever()
    {
        Debug.Log("PlayFever");
        oneShotSource.PlayOneShot(feverstart);
    }

    public void PlayMaydayMayday()
    {
        Debug.Log("MaydayMayday");
        oneShotSource.PlayOneShot(maydaymayday);
    }

    public void PlayError()
    {
    }

    void Awake()
    {
        instance = this;
    }

    public void SetEngineVolume(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        ascendingLoopSource.volume = ratio;
        descendingLoopSource.volume = 1.0f - ratio;
        AmbientSound1.volume = 0.5f;
    }

    public void StopTimeAndMuteAudioMixer()
    {
        BalloonDebug.Log("StopTimeAndMuteAudioMixer() called.");
        Time.timeScale = 0.0f;
        audioMixer.SetFloat("MasterVolume", -80.0f);
    }

    public void ResumeToNormalTimeAndResumeAudioMixer()
    {
        BalloonDebug.Log("ResumeToNormalTimeAndResumeAudioMixer() called.");
        Time.timeScale = 1.0f;
        audioMixer.SetFloat("MasterVolume", 0.0f);
    }
#pragma warning disable CS0414
    [SerializeField]
    AudioClip dash_continuous;

    [SerializeField]
    AudioClip going_up;
#pragma warning restore CS0414
}