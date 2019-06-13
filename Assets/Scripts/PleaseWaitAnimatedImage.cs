using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PleaseWaitAnimatedImage : MonoBehaviour {
    [SerializeField] float rotateSpeed = 10;
    [SerializeField] Image image = null;
    void Update() {
        transform.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);
        image.fillAmount = Mathf.PingPong(Time.time * 2, 1.0f);
    }
}
