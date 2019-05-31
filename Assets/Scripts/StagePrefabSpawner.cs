using UnityEngine;

public class StagePrefabSpawner : MonoBehaviour {
    public static string PrefabPathToLoad;
    [SerializeField] string prefabName = "";

    public string PrefabName => prefabName;

    void Awake() {
        if (string.IsNullOrEmpty(PrefabPathToLoad)) {
            PrefabPathToLoad = "Levels/Stage 01";
        }
        SushiDebug.Log($"Instantiating stage prefab resource '{PrefabPathToLoad}'...");
        var prefab = Resources.Load<GameObject>(PrefabPathToLoad);
        prefabName = prefab.name;
        var stage = Instantiate(prefab).GetComponent<Stage>();
        GameObject.Find("Canvas (Tutorial)").SetActive(stage.Tutorial);
    }

    void Start() {
        var stageName = GameObject.FindObjectOfType<StageName>();
        stageName.SetStageName(PrefabPathToLoad);
    }
}
