//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Receives float from UI Slider, and displays the value as a text.
/// </summary>
public class RCC_UISliderTextReader : MonoBehaviour {

	public Slider slider;
	public Text text;

	void Awake () {

		if(!slider)
			slider = GetComponentInParent<Slider> ();
		
		if(!text)
			text = GetComponentInChildren<Text> (); 
	
	}

	void Update () {
		
		text.text = slider.value.ToString ("F1");

	}

}
