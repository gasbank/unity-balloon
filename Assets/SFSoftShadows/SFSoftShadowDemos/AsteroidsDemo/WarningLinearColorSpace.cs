using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Random=UnityEngine.Random;

public class WarningLinearColorSpace : MonoBehaviour {

	public Text warningText;

	public void Start(){
		warningText.enabled = QualitySettings.activeColorSpace != ColorSpace.Linear;
	}

}
