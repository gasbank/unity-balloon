using UnityEngine;

public class FinishLine : MonoBehaviour {
    [SerializeField] Transform balloon = null;
    [SerializeField] HotairBalloon hotairBalloon = null;

    void Awake() {
        balloon = FindObjectOfType<BalloonLimiter>().transform;
        hotairBalloon = FindObjectOfType<HotairBalloon>();
    }

    void Update() {
        if (balloon.position.y > transform.position.y && hotairBalloon.IsStageFinished == false) {
            hotairBalloon.IsStageFinished = true;
            BalloonSound.instance.PlayGoalIn();
            BalloonSound.instance.PlayCheer();
        }
    }
}
