using UnityEngine;
using UnityEngine.UI;

public class PleaseWaitAnimatedImage : MonoBehaviour
{
    [SerializeField]
    Image image;

    [SerializeField]
    float rotateSpeed = 10;

    void Update()
    {
        transform.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);
        image.fillAmount = Mathf.PingPong(Time.time * 2, 1.0f);
    }
}