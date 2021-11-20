using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField]
    Transform balloon;

    [SerializeField]
    HotairBalloon hotairBalloon;

    void Awake()
    {
        UpdateReferences();
    }

    public void UpdateReferences()
    {
        balloon = FindObjectOfType<BalloonLimiter>().transform;
        hotairBalloon = FindObjectOfType<HotairBalloon>();
    }

    void Update()
    {
        if (balloon.position.y > transform.position.y && hotairBalloon.IsStageFinished == false)
        {
            hotairBalloon.IsStageFinished = true;
            BalloonSound.instance.PlayGoalIn();
            BalloonSound.instance.PlayCheer();
        }
    }
}