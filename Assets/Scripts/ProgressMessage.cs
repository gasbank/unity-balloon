﻿using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ProgressMessage : MonoBehaviour
{
    public static ProgressMessage instance;
    public TextMeshProUGUI messageText;

    [SerializeField]
    Subcanvas subcanvas;

    void Awake()
    {
        if (transform.parent.childCount - 1 != transform.GetSiblingIndex())
            Debug.LogError("Progress Message should be last sibling!");
    }

    // void OnValidate() {
    //     subcanvas = GetComponent<Subcanvas>();
    // }

    void OpenPopup()
    {
        // should receive message even if there is nothing to do
    }

    void ClosePopup()
    {
        // should receive message even if there is nothing to do
    }

    public void Open(string msg)
    {
        messageText.text = msg;
        //gameObject.SetActive(true);
        subcanvas.Open();
    }

    public void Close()
    {
        //gameObject.SetActive(false);
        subcanvas.Close();
    }
}