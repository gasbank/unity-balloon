﻿using UnityEngine;

public class Gear : MonoBehaviour
{
    void Update()
    {
        //transform.Rotate()
        transform.Rotate(Vector3.forward * Time.deltaTime * 100);
    }
}