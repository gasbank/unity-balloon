using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text3D = TMPro.TextMeshPro;

public class CheckpointLine : MonoBehaviour {
    [SerializeField] Text3D checkpointLineText = null;

    public string CheckpointText {
        get => checkpointLineText.text;
        set => checkpointLineText.text = value;
    }
}
