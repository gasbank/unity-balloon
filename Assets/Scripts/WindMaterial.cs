using UnityEngine;

public class WindMaterial : MonoBehaviour
{
    [SerializeField]
    Renderer planeRenderer;

    [SerializeField]
    WindRegion windRegion;

    void Awake()
    {
        planeRenderer.material = Instantiate(planeRenderer.material);
    }

    void Update()
    {
        planeRenderer.material.SetFloat("_Center", 1.0f - Mathf.Repeat(Time.time * windRegion.Power, 1.0f));
        planeRenderer.material.SetFloat("_Alpha", windRegion.CapacityRatio);
    }
}