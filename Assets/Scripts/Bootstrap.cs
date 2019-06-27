﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour {
    static readonly string LAST_PLAYED_STAGE_NUMBER = "LAST_PLAYED_STAGE_NUMBER";
    static readonly int LAST_MANUAL_STAGE_NUMBER = 25; // 손수 만든 마지막 스테이지 번호 (Stage 01 ~ Stage 25). 이 이후부터는 자동 생성된 스테이지를 이용한다.
    
    public static string GetStageSceneName(int stageNumber) {
        return $"Stage {stageNumber:d2}";
    }

    public static int LastPlayedStageNumber {
        get => PlayerPrefs.GetInt(LAST_PLAYED_STAGE_NUMBER, 1);
        set {
            PlayerPrefs.SetInt(LAST_PLAYED_STAGE_NUMBER, Mathf.Clamp(value, 1, 99999));
            PlayerPrefs.Save();
        }
    }

    public static int GetStageNumberSafe(string sceneName) {
        if (GetStageNumber(sceneName, out var result)) {
            return result;
        }
        return 1;
    }

    public static int GetStageNumber(string sceneName) {
        if (GetStageNumber(sceneName, out var result)) {
            return result;
        }
        return -1;
    }

    public static bool GetStageNumber(string sceneName, out int stageNumber) {
        return int.TryParse(sceneName.Substring("Stage ".Length), out stageNumber);
    }

    public static string CurrentStageName {
        get {
            var stagePrefabSpawner = GameObject.FindObjectOfType<StagePrefabSpawner>();
            if (stagePrefabSpawner != null) {
                if (stagePrefabSpawner.IsAutoGenerated) {
                    return $"Stage {StagePrefabSpawner.AutoGeneratedStageNumber}";
                } else {
                    return stagePrefabSpawner.PrefabName;
                }
            } else {
                return SceneManager.GetActiveScene().name;
            }
        }
    }

    public static int CurrentStageNumberSafe => GetStageNumberSafe(CurrentStageName);

    public static int CurrentStageNumber => GetStageNumber(CurrentStageName);

    static public void LoadStageScene(int stageNumber) {
        if (stageNumber <= LAST_MANUAL_STAGE_NUMBER) {
            StagePrefabSpawner.PrefabPathToLoad = "Levels/" + GetStageSceneName(stageNumber);
            StagePrefabSpawner.AutoGeneratedStageNumber = 0;
        } else {
            StagePrefabSpawner.PrefabPathToLoad = StagePrefabSpawner.AutoGeneratedStagePrefabName;
            StagePrefabSpawner.AutoGeneratedStageNumber = stageNumber;
        }
        SceneManager.LoadScene("Stage");
    }

    void Awake() {
        LoadStageScene(LastPlayedStageNumber);
    }

    static public void ReloadCurrentScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
