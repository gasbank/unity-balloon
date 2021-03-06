﻿using System.Collections;
using UnityEngine;

public class StagePrefabSpawner : MonoBehaviour {
    public static readonly string AutoGeneratedStagePrefabName = "Levels (Auto-generated)/Stage (Auto-generated)";

    private static string prefabPathToLoad;
    public static string PrefabPathToLoad {
        get => prefabPathToLoad;
        set {
            SushiDebug.Log($"PrefabPathToLoad set to {value}");
            prefabPathToLoad = value;
        }
    }

    private static int autoGeneratedStageNumber;
    public static int AutoGeneratedStageNumber {
        get => autoGeneratedStageNumber;
        set {
            SushiDebug.Log($"AutoGeneratedStageNumber set to {value}");
            autoGeneratedStageNumber = value;
        }
    }
    
    [SerializeField] string prefabName = "";
    [SerializeField] Stage stage = null;

    public string PrefabName => prefabName;
    public bool IsAutoGenerated => PrefabPathToLoad == AutoGeneratedStagePrefabName;

    IEnumerator Start() {
        if (string.IsNullOrEmpty(PrefabPathToLoad)) {
            if (string.IsNullOrEmpty(prefabName)) {
                PrefabPathToLoad = "Levels/Stage 01";
            } else {
                PrefabPathToLoad = prefabName;
            }
        }
        SushiDebug.Log($"Instantiating stage prefab resource '{PrefabPathToLoad}'...");
        var prefab = Resources.Load<GameObject>(PrefabPathToLoad);
        prefabName = prefab.name;
        stage = Instantiate(prefab).GetComponent<Stage>();
        GameObject.Find("Canvas (Tutorial)/Tutorial").GetComponent<Tutorial>().TutorialTemplate = stage.TutorialTemplate;
        GameObject.Find("Canvas (Tutorial)").SetActive(stage.TutorialTemplate != 0);

        // 아래 할당으로 새로운 스테이지가 스폰된다.
        // 새롭게 스폰된 스테이지를 변경하기 위해서는 한 프레임 기다려햔다.
        // 이러한 처리는 맨 끝의 yield return stage.PostProcessOnStageSpawn();
        // 함수에서 처리한다.
        stage.AutoGeneratedStageNumber = AutoGeneratedStageNumber;

        var stageName = GameObject.FindObjectOfType<StageName>();
        if (stage.AutoGeneratedStageNumber > 0) {
            stageName.SetStageName($"Level {stage.AutoGeneratedStageNumber}");
        } else {
            stageName.SetStageName(PrefabPathToLoad);
        }

        yield return stage.PostProcessOnStageSpawn();
    }
}
