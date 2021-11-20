using UnityEngine;
using Text3D = TMPro.TextMeshPro;

public class CheckpointLine : MonoBehaviour
{
    [SerializeField]
    Text3D checkpointLineText;

    public string CheckpointText
    {
        get => checkpointLineText.text;
        set => checkpointLineText.text = value;
    }
}