using UnityEngine;

public class FinishLine : MonoBehaviour {
    [SerializeField] Transform balloon = null;
    [SerializeField] Canvas finishGroup = null;

    void Awake() {
        balloon = FindObjectOfType<BalloonLimiter>().transform;
        finishGroup = FindObjectOfType<FinishGroup>().GetComponent<Canvas>();
    }

    void Update() {
        if (balloon.position.y > transform.position.y && finishGroup.enabled == false) {
            finishGroup.enabled = true;
            BalloonSound.instance.PlayGoalIn();
            BalloonSound.instance.PlayCheer();
        }
    }
}
