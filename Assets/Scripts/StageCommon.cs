using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageCommon : MonoBehaviour {
    [SerializeField] GameObject titleGroup = null;
    [SerializeField] GameObject titleImage = null;

    public bool IsTitleVisible => titleGroup.activeSelf;
    public static bool awaken = false;

    void Awake() {
        // 게임 첫 실행했을 때만 타이틀 이미지 보인다.
        titleImage.SetActive(awaken == false);
        awaken = true;
    }

    public void DeactivateTitleGroup() {
        titleGroup.SetActive(false);
    }
}
