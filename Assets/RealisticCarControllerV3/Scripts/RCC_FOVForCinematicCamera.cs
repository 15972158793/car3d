﻿//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Animation attached to "Animation Pivot" of the Cinematic Camera is feeding FOV float value.
/// </summary>
public class RCC_FOVForCinematicCamera : MonoBehaviour {

	private RCC_CinematicCamera cinematicCamera;
	public float FOV = 30f;

	void Awake () {

		cinematicCamera = GetComponentInParent<RCC_CinematicCamera> ();
	
	}

	void Update () {

		cinematicCamera.targetFOV = FOV;
	
	}

}
