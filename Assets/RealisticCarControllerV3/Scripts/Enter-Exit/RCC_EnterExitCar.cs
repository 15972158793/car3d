﻿//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCC_EnterExitCar : MonoBehaviour {

	// Getting an Instance of Main Shared RCC Settings.
	#region RCC Settings Instance

	private RCC_Settings RCCSettingsInstance;
	private RCC_Settings RCCSettings {
		get {
			if (RCCSettingsInstance == null) {
				RCCSettingsInstance = RCC_Settings.Instance;
			}
			return RCCSettingsInstance;
		}
	}

	#endregion

	private RCC_CarControllerV3 carController;
	private GameObject carCamera;
	private GameObject player;
	private GameObject dashboard;
	public Transform getOutPosition;

	public bool isPlayerIn = false;
	private bool  opened = false;
	private float waitTime = 1f;
	private bool  temp = false;
	
	void Awake (){

		carController = GetComponent<RCC_CarControllerV3>();
		carCamera = GameObject.FindObjectOfType<RCC_Camera>().gameObject;
	
		if(GameObject.FindObjectOfType<RCC_DashboardInputs>())
			dashboard = GameObject.FindObjectOfType<RCC_DashboardInputs>().gameObject;

		if(!getOutPosition){
			GameObject getOutPos = new GameObject("Get Out Position");
			getOutPos.transform.SetParent(transform);
			getOutPos.transform.localPosition = new Vector3(-1.5f, 0f, 0f);
			getOutPos.transform.localRotation = Quaternion.identity;
			getOutPosition = getOutPos.transform;
		}

	}

	void Start(){

		if(dashboard)
			StartCoroutine("DisableDashboard");

	}

	IEnumerator DisableDashboard(){

		yield return new WaitForEndOfFrame();
		dashboard.SetActive(false);

	}
	
	void Update (){

		if(Input.GetKeyDown(RCCSettings.enterExitVehicleKB) && opened && !temp){
			GetOut();
			opened = false;
			temp = false;
		}

		if(!isPlayerIn)
			carController.canControl = false;

	}
	
	IEnumerator Act (GameObject fpsplayer){
		
		player = fpsplayer;

		if (!opened && !temp){
			GetIn();
			opened = true;
			temp = true;
			yield return new WaitForSeconds(waitTime);
			temp = false;
		}

	}
	
	void GetIn (){

		isPlayerIn = true;

		carCamera.SetActive(true);

		if(carCamera.GetComponent<RCC_Camera>()){
			carCamera.GetComponent<RCC_Camera>().cameraSwitchCount = 10;
			carCamera.GetComponent<RCC_Camera>().ChangeCamera();
		}

		carCamera.transform.GetComponent<RCC_Camera>().SetPlayerCar(gameObject);
		player.transform.SetParent(transform);
		player.transform.localPosition = Vector3.zero;
		player.transform.localRotation = Quaternion.identity;
		player.SetActive(false);
		carController.canControl = true;

		if(dashboard){
			dashboard.SetActive(true);
			dashboard.GetComponent<RCC_DashboardInputs>().GetVehicle(carController);
		}

			if(!carController.engineRunning)
				SendMessage ("StartEngine");
		
		Cursor.lockState = CursorLockMode.None;

	}
	
	void GetOut (){

		isPlayerIn = false;

		player.transform.SetParent(null);
		player.transform.position = getOutPosition.position;
		player.transform.rotation = getOutPosition.rotation;
		player.transform.rotation = Quaternion.Euler (0f, player.transform.eulerAngles.y, 0f);
		carCamera.SetActive(false);
		player.SetActive(true);

		carController.canControl = false;

		if(!RCCSettings.keepEnginesAlive)
			carController.engineRunning = false;
		if(dashboard)
			dashboard.SetActive(false);
		
		Cursor.lockState = CursorLockMode.Locked;

	}
	
}