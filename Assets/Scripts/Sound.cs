using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class Sound : MonoBehaviour {
    public static Sound instance;
    [SerializeField] private AudioClip buttonClick = null;
    [SerializeField] private AudioClip balloonMade = null;
    [SerializeField] private AudioClip balloonNew = null;
    [SerializeField] private AudioClip wipeStain = null;
    [SerializeField] private AudioClip wipeStainFinish = null;
    [SerializeField] private AudioClip tadaA = null;
    [SerializeField] private AudioClip tadaF = null;
    [SerializeField] private AudioClip tadaG = null;
    [SerializeField] private AudioClip softTada = null;
    [SerializeField] private AudioClip error = null;
    [SerializeField] private AudioClip snap = null;
    [SerializeField] private AudioClip snappyClick = null;
    [SerializeField] private AudioClip rubberImpact = null;
    [SerializeField] private AudioClip correctlyFinished = null;
    [SerializeField] private AudioClip correctlyFinishedMild = null;
    //[SerializeField] private AudioClip normalBgm = null;
    [SerializeField] private AudioClip whacACatBgm = null;
    [SerializeField] private AudioClip feverBgm = null;
    [SerializeField] private AudioClip whooshAir = null;
    [SerializeField] private AudioClip jingleAchievement = null;
    [SerializeField] private AudioClip errorBuzzer = null;
    [SerializeField] private AudioClip gatherStoredMax = null;
    [SerializeField] private AudioClip longTada = null;
    [SerializeField] private AudioClip dingaling = null;
    [SerializeField] private AudioSource bgmAudioSource = null;
    [SerializeField] private AudioSource sfxAudioSource = null;
    [SerializeField] private AudioSource gatherStoredMaxSfxAudioSource = null;
    [SerializeField] AudioMixer audioMixer = null;

    public bool BgmAudioSourceActive { get { return bgmAudioSource.enabled; } set { bgmAudioSource.enabled = value; } }
    public bool SfxAudioSourceActive { get { return sfxAudioSource.enabled; } set { sfxAudioSource.enabled = value; } }
    public bool GatherStoredMaxSfxEnabled { get; set; }

    public void PlayButtonClick() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.buttonClick); } }
    public void PlayBalloonMade() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.balloonMade); } }
    public void PlayBalloonNew() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.balloonNew); } }
    public void PlayWipeStain() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.wipeStain); } }
    public void PlayWipeStainFinish() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.wipeStainFinish); } }
    public void PlayTadaA() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.tadaA); } }
    public void PlayTadaF() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.tadaF); } }
    public void PlayTadaG() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.tadaG); } }
    public void PlaySoftTada() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.softTada); } }
    public void PlayError() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.error); } }
    public void PlaySnap() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.snap); } }
    public void PlaySnappyClick() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.snappyClick); } }
    public void PlayRubberImpact() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.rubberImpact); } }
    public void PlayCorrectlyFinished() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.correctlyFinished); } }
    public void PlayCorrectlyFinishedMild() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.correctlyFinishedMild); } }
    public void PlayWhooshAir() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.whooshAir); } }
    public void PlayJingleAchievement() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.jingleAchievement); } }
    public void PlayErrorBuzzer() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.errorBuzzer); } }
    public void PlayGatherStoredMax() { if (GatherStoredMaxSfxEnabled) { instance.gatherStoredMaxSfxAudioSource.PlayOneShot(instance.gatherStoredMax); } }
    public void PlayLongTada() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.longTada); } }
    public void PlayDingaling() { if (SfxAudioSourceActive) { instance.sfxAudioSource.PlayOneShot(instance.dingaling); } }

    public void PlayWhacACatBgm() { if (BgmAudioSourceActive) { instance.bgmAudioSource.Stop(); instance.bgmAudioSource.PlayOneShot(instance.whacACatBgm); } }
    public void PlayNormalBgm() { if (BgmAudioSourceActive) { instance.bgmAudioSource.enabled = false; instance.bgmAudioSource.enabled = true; } }
    public void PlayFeverBgm() { if (BgmAudioSourceActive) { instance.bgmAudioSource.Stop(); instance.bgmAudioSource.PlayOneShot(instance.feverBgm); } }

    public void StopTimeAndMuteAudioMixer() {
        if (Time.timeScale != 1) {
            Debug.LogError("Time.timeScale expected to be 1 at this moment!");
        }
        Time.timeScale = 0;
        audioMixer.SetFloat("MasterVolume", -80.0f);
    }

    public void ResumeToNormalTimeAndResumeAudioMixer() {
        Time.timeScale = 1;
        audioMixer.SetFloat("MasterVolume", 0.0f);
    }

    public void EnableMasterVolume(bool b) {
        audioMixer.SetFloat("MasterVolume", b ? 0.0f : -80.0f);
    }

    public void EnableBgmVolume(bool b) {
        SushiDebug.Log($"EnableBgmVolume {b}");
        audioMixer.SetFloat("BgmVolume", b ? 0.0f : -80.0f);
    }

    public void EnableSfxVolume(bool b) {
        SushiDebug.Log($"EnableSfxVolume {b}");
        audioMixer.SetFloat("SfxVolume", b ? 0.0f : -80.0f);
    }

    public void EnableSfxSpecialVolume(bool b) {
        SushiDebug.Log($"EnableSfxSpecialVolume {b}");
        audioMixer.SetFloat("SfxSpecialVolume", b ? 0.0f : -80.0f);
    }
}
