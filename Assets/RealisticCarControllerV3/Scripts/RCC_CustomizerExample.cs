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
using UnityEngine.EventSystems;

/// <summary>
/// A simple customizer example script used for receiving methods from UI elements and send them to RCC_Customization script. Also updates all UI elements for new spawned cars too.
/// </summary>
public class RCC_CustomizerExample : MonoBehaviour {
	
	[Header("Current Car")]
	public RCC_CarControllerV3 car;

	[Header("UI Menus")]
	public GameObject wheelsMenu;
	public GameObject configurationMenu;
	public GameObject steeringAssistancesMenu;
	public GameObject colorsMenu;

	[Header("UI Sliders")]
	public Slider frontCamber;
	public Slider rearCamber;
	public Slider frontSuspensionDistances;
	public Slider rearSuspensionDistances;
	public Slider frontSuspensionDampers;
	public Slider rearSuspensionDampers;
	public Slider frontSuspensionSprings;
	public Slider rearSuspensionSprings;
	public Slider gearShiftingThreshold;
	public Slider clutchThreshold;

	[Header("UI Toggles")]
	public Toggle TCS;
	public Toggle ABS;
	public Toggle ESP;
	public Toggle SH;
	public Toggle counterSteering;
	public Toggle steeringSensitivity;
	public Toggle NOS;
	public Toggle turbo;
	public Toggle exhaustFlame;
	public Toggle revLimiter;
	public Toggle clutchMargin;
	public Toggle transmission;

	[Header("UI InputFields")]
	public InputField maxSpeed;
	public InputField maxBrake;
	public InputField maxTorque;

	[Header("UI Dropdown Menus")]
	public Dropdown drivetrainMode;

	void Start(){

		CheckUIs ();

		if (!car) {
			if (GameObject.FindObjectOfType<RCC_Camera> () && GameObject.FindObjectOfType<RCC_Camera> ().playerCar)
				car = GameObject.FindObjectOfType<RCC_Camera> ().playerCar.GetComponent<RCC_CarControllerV3>();
		}

	}

	public void CheckUIs (){

		if (!car)
			return;

		frontCamber.value = car.FrontLeftWheelCollider.camber;
		rearCamber.value = car.RearLeftWheelCollider.camber;
		frontSuspensionDistances.value = car.FrontLeftWheelCollider.wheelCollider.suspensionDistance;
		rearSuspensionDistances.value = car.RearLeftWheelCollider.wheelCollider.suspensionDistance;
		frontSuspensionDampers.value = car.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper;
		rearSuspensionDampers.value = car.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper;
		frontSuspensionSprings.value = car.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring;
		rearSuspensionSprings.value = car.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring;
		gearShiftingThreshold.value = car.gearShiftingThreshold;
		clutchThreshold.value = car.clutchInertia;

		TCS.isOn = car.TCS;
		ABS.isOn = car.ABS;
		ESP.isOn = car.ESP;
		SH.isOn = car.steeringHelper;
		counterSteering.isOn = car.applyCounterSteering;
		steeringSensitivity.isOn = car.steerAngleSensitivityAdjuster;
		NOS.isOn = car.useNOS;
		turbo.isOn = car.useTurbo;
		exhaustFlame.isOn = car.useExhaustFlame;
		revLimiter.isOn = car.useRevLimiter;
		clutchMargin.isOn = car.useClutchMarginAtFirstGear;
		transmission.isOn = RCC_Settings.Instance.useAutomaticGear;

		maxSpeed.text = car.maxspeed.ToString();
		maxBrake.text = car.brakeTorque.ToString();
		maxTorque.text = car.engineTorque.ToString();

		switch (car._wheelTypeChoise) {

		case RCC_CarControllerV3.WheelType.FWD:
			drivetrainMode.value = 0;
			break;

		case RCC_CarControllerV3.WheelType.RWD:
			drivetrainMode.value = 1;
			break;

		case RCC_CarControllerV3.WheelType.AWD:
			drivetrainMode.value = 2;
			break;

		case RCC_CarControllerV3.WheelType.BIASED:
			drivetrainMode.value = 3;
			break;

		}


	}

	public void OpenMenu(GameObject activeMenu){

		if (activeMenu.activeInHierarchy) {
			activeMenu.SetActive (false);
			return;
		}

		wheelsMenu.SetActive (false);
		configurationMenu.SetActive (false);
		steeringAssistancesMenu.SetActive (false);
		colorsMenu.SetActive (false);

		activeMenu.SetActive (true);

	}

	public void CloseAllMenus(){

		wheelsMenu.SetActive (false);
		configurationMenu.SetActive (false);
		steeringAssistancesMenu.SetActive (false);
		colorsMenu.SetActive (false);

	}

	public void SetCustomizationMode (bool state) {

		RCC_Customization.SetCustomizationMode (car, state);

		if(state)
			CheckUIs ();

	}

	public void SetFrontCambersBySlider (Slider slider) {

		RCC_Customization.SetFrontCambers (car, slider.value);
	
	}

	public void SetRearCambersBySlider (Slider slider) {

		RCC_Customization.SetRearCambers (car, slider.value);

	}

	public void TogglePreviewSmokeByToggle (Toggle toggle){

		RCC_Customization.SetSmokeParticle (car, toggle.isOn);

	}

	public void TogglePreviewExhaustFlameByToggle (Toggle toggle){

		RCC_Customization.SetExhaustFlame (car, toggle.isOn);

	}

	public void SetSmokeColorByColorPicker (ColorPickerBySliders color){

		RCC_Customization.SetSmokeColor (car, 0, color.color);

	}

	public void SetHeadlightColorByColorPicker (ColorPickerBySliders color){

		RCC_Customization.SetHeadlightsColor (car, color.color);

	}

	public void ChangeWheelsBySlider (Slider slider){

		RCC_Customization.ChangeWheels (car, RCC_ChangableWheels.Instance.wheels[(int)slider.value].wheel);

	}

	public void SetFrontSuspensionTargetsBySlider (Slider slider){

		RCC_Customization.SetFrontSuspensionsTargetPos (car, slider.value);

	}

	public void SetRearSuspensionTargetsBySlider (Slider slider){

		RCC_Customization.SetRearSuspensionsTargetPos (car, slider.value);

	}

	public void SetFrontSuspensionDistancesBySlider (Slider slider){

		RCC_Customization.SetFrontSuspensionsDistances (car, slider.value);

	}

	public void SetRearSuspensionDistancesBySlider (Slider slider){

		RCC_Customization.SetRearSuspensionsDistances (car, slider.value);

	}

	public void SetGearShiftingThresholdBySlider (Slider slider){

		RCC_Customization.SetGearShiftingThreshold (car, Mathf.Clamp(slider.value, .5f, .95f));

	}

	public void SetClutchThresholdBySlider (Slider slider){

		RCC_Customization.SetClutchThreshold (car, Mathf.Clamp(slider.value, .1f, .9f));

	}

	public void SetDriveTrainModeByDropdown (Dropdown dropdown){

		switch (dropdown.value) {

		case 0:
			RCC_Customization.SetDrivetrainMode (car, RCC_CarControllerV3.WheelType.FWD);
			break;

		case 1:
			RCC_Customization.SetDrivetrainMode (car, RCC_CarControllerV3.WheelType.RWD);
			break;

		case 2:
			RCC_Customization.SetDrivetrainMode (car, RCC_CarControllerV3.WheelType.AWD);
			break;

		}

	}

	public void SetSteeringSensitivityByToggle (Toggle toggle){

		RCC_Customization.SetSteeringSensitivity (car, toggle.isOn);

	}

	public void SetCounterSteeringByToggle (Toggle toggle){

		RCC_Customization.SetCounterSteering (car, toggle.isOn);

	}

	public void SetNOSByToggle (Toggle toggle){

		RCC_Customization.SetNOS (car, toggle.isOn);

	}

	public void SetTurboByToggle (Toggle toggle){

		RCC_Customization.SetTurbo (car, toggle.isOn);

	}

	public void SetExhaustFlameByToggle (Toggle toggle){

		RCC_Customization.SetUseExhaustFlame (car, toggle.isOn);

	}

	public void SetRevLimiterByToggle (Toggle toggle){

		RCC_Customization.SetRevLimiter (car, toggle.isOn);

	}

	public void SetClutchMarginByToggle (Toggle toggle){

		RCC_Customization.SetClutchMargin (car, toggle.isOn);

	}

	public void SetFrontSuspensionsSpringForceBySlider (Slider slider){

		RCC_Customization.SetFrontSuspensionsSpringForce (car, Mathf.Clamp(slider.value, 10000f, 100000f));

	}

	public void SetRearSuspensionsSpringForceBySlider (Slider slider){

		RCC_Customization.SetRearSuspensionsSpringForce (car, Mathf.Clamp(slider.value, 10000f, 100000f));

	}

	public void SetFrontSuspensionsSpringDamperBySlider (Slider slider){

		RCC_Customization.SetFrontSuspensionsSpringDamper (car, Mathf.Clamp(slider.value, 1000f, 10000f));

	}

	public void SetRearSuspensionsSpringDamperBySlider (Slider slider){

		RCC_Customization.SetRearSuspensionsSpringDamper (car, Mathf.Clamp(slider.value, 1000f, 10000f));

	}

	public void SetMaximumSpeedByInputField (InputField inputField){

		RCC_Customization.SetMaximumSpeed (car, StringToFloat(inputField.text, 200f));
		inputField.text = car.maxspeed.ToString ();

	}

	public void SetMaximumTorqueByInputField (InputField inputField){

		RCC_Customization.SetMaximumTorque (car, StringToFloat(inputField.text, 2000f));
		inputField.text = car.engineTorque.ToString ();

	}

	public void SetMaximumBrakeByInputField (InputField inputField){

		RCC_Customization.SetMaximumBrake (car, StringToFloat(inputField.text, 2000f));
		inputField.text = car.brakeTorque.ToString ();

	}

	public void RepairCar (){

		RCC_Customization.RepairCar (car);

	}

	public void SetESP(Toggle toggle){

		RCC_Customization.SetESP (car, toggle.isOn);

	}
		
	public void SetABS(Toggle toggle){

		RCC_Customization.SetABS (car, toggle.isOn);

	}

	public void SetTCS(Toggle toggle){

		RCC_Customization.SetTCS (car, toggle.isOn);

	}

	public void SetSH(Toggle toggle){

		RCC_Customization.SetSH (car, toggle.isOn);

	}

	public void SetSHStrength(Slider slider){

		RCC_Customization.SetSHStrength (car, slider.value);

	}

	public void CameraOrbitDrag(BaseEventData data){

		RCC_Customization.SetCameraOrbitOnDrag (data);

	}

	public void SetTransmission(Toggle toggle){

		RCC_Customization.SetTransmission (toggle.isOn);

	}

	public void SaveStats(){

		RCC_Customization.SaveStats (car);

	}

	public void LoadStats(){

		RCC_Customization.LoadStats (car);
		CheckUIs ();

	}

	private float StringToFloat(string stringValue, float defaultValue){
		
		float result = defaultValue;
		float.TryParse(stringValue, out result);
		return result;

	}

}
