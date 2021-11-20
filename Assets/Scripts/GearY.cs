using UnityEngine;

public class GearY : MonoBehaviour
{
    void Update()
    {
        //transform.Rotate()
        transform.Rotate(Vector3.up * Time.deltaTime * 100);
    }
}