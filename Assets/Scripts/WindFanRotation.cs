using UnityEngine;

public class WindFanRotation : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed = 10.0f;

    [SerializeField]
    WindRegion windRegion;

    void Update()
    {
        transform.Rotate(Vector3.forward * Time.deltaTime * rotationSpeed * windRegion.Power *
                         windRegion.CapacityRatio);
    }
}