//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Receiving inputs from active car on your scene, and feeds dashboard needles, texts, images.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/Dashboard Inputs")]
public class RCC_DashboardInputs : MonoBehaviour {

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

	public RCC_CarControllerV3 carController;

	public GameObject RPMNeedle;
	public GameObject KMHNeedle;
	public GameObject turboGauge;
	public GameObject NOSGauge;
	public GameObject BoostNeedle;
	public GameObject NoSNeedle;

	private float RPMNeedleRotation = 0f;
	private float KMHNeedleRotation = 0f;
	private float BoostNeedleRotation = 0f;
	private float NoSNeedleRotation = 0f;

	internal float RPM;
	internal float KMH;
	internal int direction = 1;
	internal float Gear;
	internal bool changingGear = false;
	internal bool NGear = false;

	internal bool ABS = false;
	internal bool ESP = false;
	internal bool Park = false;
	internal bool Headlights = false;

	internal RCC_CarControllerV3.IndicatorsOn indicators;

	void OnEnable(){
		
		RCC_CarControllerV3.OnRCCPlayerSpawned += RCC_CarControllerV3_OnRCCSpawned;

	}

	void RCC_CarControllerV3_OnRCCSpawned (RCC_CarControllerV3 RCC){
		
		GetVehicle (RCC);

	}

	void Update(){

		if(RCCSettings.uiType == RCC_Settings.UIType.None){
			gameObject.SetActive(false);
			enabled = false;
			return;
		}

		GetValues();

	}
	
	public void GetVehicle(RCC_CarControllerV3 rcc){

		carController = rcc;
		RCC_UIDashboardButton[] buttons = GameObject.FindObjectsOfType<RCC_UIDashboardButton>();

		foreach(RCC_UIDashboardButton button in buttons)
			button.Check();

	}

	void GetValues(){

		if(!carController)
			return;

		if(!carController.canControl || carController.AIController){
			return;
		}

		if(NOSGauge){
			if(carController.useNOS){
				if(!NOSGauge.activeSelf)
					NOSGauge.SetActive(true);
			}else{
				if(NOSGauge.activeSelf)
					NOSGauge.SetActive(false);
			}
		}

		if(turboGauge){
			if(carController.useTurbo){
				if(!turboGauge.activeSelf)
					turboGauge.SetActive(true);
			}else{
				if(turboGauge.activeSelf)
					turboGauge.SetActive(false);
			}
		}
		
		RPM = carController.engineRPM;
		KMH = carController.speed;
		direction = carController.direction;
		Gear = carController.currentGear;

		changingGear = carController.changingGear;
		NGear = carController.NGear;
		
		ABS = carController.ABSAct;
		ESP = carController.ESPAct;
		Park = carController.handbrakeInput > .1f ? true : false;
		Headlights = carController.lowBeamHeadLightsOn || carController.highBeamHeadLightsOn;
		indicators = carController.indicatorsOn;

		if(RPMNeedle){
			RPMNeedleRotation = (carController.engineRPM / 50f);
			RPMNeedle.transform.eulerAngles = new Vector3(RPMNeedle.transform.eulerAngles.x ,RPMNeedle.transform.eulerAngles.y, -RPMNeedleRotation);
		}
		if(KMHNeedle){
			if(RCCSettings.units == RCC_Settings.Units.KMH)
				KMHNeedleRotation = (carController.speed);
			else
				KMHNeedleRotation = (carController.speed * 0.62f);
			KMHNeedle.transform.eulerAngles = new Vector3(KMHNeedle.transform.eulerAngles.x ,KMHNeedle.transform.eulerAngles.y, -KMHNeedleRotation);
		}
		if(BoostNeedle){
			BoostNeedleRotation = (carController.turboBoost / 30f) * 270f;
			BoostNeedle.transform.eulerAngles = new Vector3(BoostNeedle.transform.eulerAngles.x ,BoostNeedle.transform.eulerAngles.y, -BoostNeedleRotation);
		}
		if(NoSNeedle){
			NoSNeedleRotation = (carController.NoS / 100f) * 270f;
			NoSNeedle.transform.eulerAngles = new Vector3(NoSNeedle.transform.eulerAngles.x ,NoSNeedle.transform.eulerAngles.y, -NoSNeedleRotation);
		}
			
	}

	void OnDisable(){

		RCC_CarControllerV3.OnRCCPlayerSpawned -= RCC_CarControllerV3_OnRCCSpawned;

	}

}



