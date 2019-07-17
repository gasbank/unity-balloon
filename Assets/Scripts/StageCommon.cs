using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageCommon : MonoBehaviour {
    [SerializeField] GameObject titleGroup = null;
    [SerializeField] GameObject titleImage = null; 

   [SerializeField] Image image;

    public bool IsTitleVisible => titleGroup.activeSelf;
    public static bool awaken = false;
   
    void Awake() {
        // 게임 첫 실행했을 때만 타이틀 이미지 보인다.
        titleImage.SetActive(awaken == false);
        awaken = true;

    }
bool bfadeout = false;
    public void DeactivateTitleGroup() 
    {

         

        if(image.color != null)    
        {
        image.color -= new Color(0,0,0,Time.deltaTime) ;
       // image.color.a -= Time.deltaTime;
       // titleImage.a = color.a ;
        bfadeout = true;
       if(image.color.a <= 0)
        titleGroup.SetActive(false);
      
       }
       
    
    }

    public void Update()
    {
        if(bfadeout == true)
        DeactivateTitleGroup();
    }
}
