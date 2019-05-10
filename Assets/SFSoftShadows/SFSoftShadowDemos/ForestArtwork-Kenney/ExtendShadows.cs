using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendShadows : MonoBehaviour {
	private void Start(){
		// Extend the SFSS extents past the bottom of the screen for the sampling on the trees.
		GetComponent<SFRenderer>().extents = Rect.MinMaxRect(-1, -1.5f, 1, 1);
	}
}
