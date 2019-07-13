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
/// Receiving inputs from UI buttons, and feeds active cars on your scene.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/Mobile/Mobile Buttons")]
public class RCC_MobileButtons : MonoBehaviour {

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

	public RCC_CarControllerV3[] carControllers;

	public RCC_UIController gasButton;
	public RCC_UIController brakeButton;
	public RCC_UIController leftButton;
	public RCC_UIController rightButton;
	public RCC_UISteeringWheelController steeringWheel;
	public RCC_UIController handbrakeButton;
	public RCC_UIController NOSButton;
	public GameObject gearButton;

	private float gasInput = 0f;
	private float brakeInput = 0f;
	private float leftInput = 0f;
	private float rightInput = 0f;
	private float steeringWheelInput = 0f;
	private float handbrakeInput = 0f;
	private float NOSInput = 1f;
	private float gyroInput = 0f;

	private Vector3 orgBrakeButtonPos;



	void Start(){

		if(RCCSettings.controllerType != RCC_Settings.ControllerType.Mobile){
			
			if(gasButton)
				gasButton.gameObject.SetActive(false);
			if(leftButton)
				leftButton.gameObject.SetActive(false);
			if(rightButton)
				rightButton.gameObject.SetActive(false);
			if(brakeButton)
				brakeButton.gameObject.SetActive(false);
			if(steeringWheel)
				steeringWheel.gameObject.SetActive(false);
			if(handbrakeButton)
				handbrakeButton.gameObject.SetActive(false);
			if(NOSButton)
				NOSButton.gameObject.SetActive(false);
			if(gearButton)
				gearButton.gameObject.SetActive(false);
			
			enabled = false;
			return;

		}

		orgBrakeButtonPos = brakeButton.transform.position;

	}

	void OnEnable(){

		RCC_CarControllerV3.OnRCCPlayerSpawned += RCC_CarControllerV3_OnRCCSpawned;
		GetVehicles();

	}

	void RCC_CarControllerV3_OnRCCSpawned (RCC_CarControllerV3 RCC){

		if (!RCC.AIController)
			GetVehicles ();
		
	}

	public void GetVehicles(){

		carControllers = GameObject.FindObjectsOfType<RCC_CarControllerV3>();

	}

	void Update(){

		if(RCCSettings.useSteeringWheelForSteering){

			if(RCCSettings.useAccelerometerForSteering)
				RCCSettings.useAccelerometerForSteering = false;
			
			if(!steeringWheel.gameObject.activeInHierarchy){
				steeringWheel.gameObject.SetActive(true);
				brakeButton.transform.position = orgBrakeButtonPos;
			}

			if(leftButton.gameObject.activeInHierarchy)
				leftButton.gameObject.SetActive(false);
			if(rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(false);
			
		}

		if(RCCSettings.useAccelerometerForSteering){

			if(RCCSettings.useSteeringWheelForSteering)
				RCCSettings.useSteeringWheelForSteering = false;

			brakeButton.transform.position = leftButton.transform.position;
			if(steeringWheel.gameObject.activeInHierarchy)
				steeringWheel.gameObject.SetActive(false);
			if(leftButton.gameObject.activeInHierarchy)
				leftButton.gameObject.SetActive(false);
			if(rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(false);
			
		}

		if(!RCCSettings.useAccelerometerForSteering && !RCCSettings.useSteeringWheelForSteering){
			
			if(steeringWheel && steeringWheel.gameObject.activeInHierarchy)
				steeringWheel.gameObject.SetActive(false);
			if(!leftButton.gameObject.activeInHierarchy){
				brakeButton.transform.position = orgBrakeButtonPos;
				leftButton.gameObject.SetActive(true);
			}
			if(!rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(true);
			
		}

		gasInput = GetInput(gasButton);
		brakeInput = GetInput(brakeButton);
		leftInput = GetInput(leftButton);
		rightInput = GetInput(rightButton);

		if(steeringWheel)
			steeringWheelInput = steeringWheel.input;

		if(RCCSettings.useAccelerometerForSteering)
			gyroInput = Input.acceleration.x * RCCSettings.gyroSensitivity;
		else
			gyroInput = 0f;
		
		handbrakeInput = GetInput(handbrakeButton);
		NOSInput = Mathf.Clamp(GetInput(NOSButton) * 2.5f, 1f, 2.5f);

		for (int i = 0; i < carControllers.Length; i++) {

			if(carControllers[i].canControl && !carControllers[i].AIController){

				carControllers[i].gasInput = gasInput;
				carControllers[i].brakeInput = brakeInput;
				carControllers[i].steerInput = -leftInput + rightInput + steeringWheelInput + gyroInput;
				carControllers[i].handbrakeInput = handbrakeInput;
				carControllers[i].boostInput = NOSInput;

			}

		}

	}

	float GetInput(RCC_UIController button){

		if(button == null)
			return 0f;

		return(button.input);

	}

	public void ChangeCamera () {

		if(GameObject.FindObjectOfType<RCC_Camera>())
			GameObject.FindObjectOfType<RCC_Camera>().ChangeCamera();

	}

	public void ChangeController(int index){

		switch(index){

		case 0:
			RCCSettings.useAccelerometerForSteering = false;
			RCCSettings.useSteeringWheelForSteering = false;
			break;
		case 1:
			RCCSettings.useAccelerometerForSteering = true;
			RCCSettings.useSteeringWheelForSteering = false;
			break;
		case 2:
			RCCSettings.useAccelerometerForSteering = false;
			RCCSettings.useSteeringWheelForSteering = true;
			break;

		}

	}

	void OnDisable(){

		RCC_CarControllerV3.OnRCCPlayerSpawned -= RCC_CarControllerV3_OnRCCSpawned;

	}

}
