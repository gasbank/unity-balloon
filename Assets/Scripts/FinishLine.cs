using UnityEngine;

public class FinishLine : MonoBehaviour {
    [SerializeField] Transform balloon = null;
    [SerializeField] GameObject finishGroup = null;

    void Update() {
        if (balloon.position.y > transform.position.y && finishGroup.activeSelf == false) {
            finishGroup.SetActive(true);
        }
    }
}
