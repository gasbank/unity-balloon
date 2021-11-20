using UnityEngine;

public class LimitCubeGroup : MonoBehaviour
{
    [SerializeField]
    Transform leftCube;

    [SerializeField]
    Transform rightCube;

    public void SetWidth(float width)
    {
        var boundThickness = rightCube.GetComponent<Collider>().bounds.extents.x;
        var cubePosX = width / 2 + boundThickness;
        rightCube.transform.position = Vector3.right * cubePosX;
        leftCube.transform.position = -Vector3.right * cubePosX;
        //SushiDebug.Log($"LimitCubeGroup.SetWidth({width})");
    }
}