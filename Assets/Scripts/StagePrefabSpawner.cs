using UnityEngine;

public class StagePrefabSpawner : MonoBehaviour {
    public static string PrefabPathToLoad;

    void Awake() {
        if (string.IsNullOrEmpty(PrefabPathToLoad)) {
            PrefabPathToLoad = "Levels/Stage 01";
        }
        SushiDebug.Log($"Instantiating stage prefab resource '{PrefabPathToLoad}'...");
        var stage = Instantiate(Resources.Load<GameObject>(PrefabPathToLoad)).GetComponent<Stage>();
        GameObject.Find("Canvas (Tutorial)").SetActive(stage.Tutorial);
    }

    void Start() {
        var stageName = GameObject.FindObjectOfType<StageName>();
        stageName.SetStageName(PrefabPathToLoad);
    }
}
