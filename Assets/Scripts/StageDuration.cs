using System;
using TMPro;
using UnityEngine;

public class StageDuration : MonoBehaviour
{
    [SerializeField]
    HotairBalloon hotairBalloon;

    [SerializeField]
    TextMeshProUGUI text;

    void Awake()
    {
        UpdateReferences();
    }

    public void UpdateReferences()
    {
        hotairBalloon = FindObjectOfType<HotairBalloon>();
    }

    void Update()
    {
        if (hotairBalloon != null)
            text.text = (DateTime.MinValue + TimeSpan.FromSeconds(hotairBalloon.StageElapsedTime))
                .ToString("mm:ss.fff");
    }
}