//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

///<summary>
/// Main Customization Class For RCC.
///</summary>
public class RCC_Customization : MonoBehaviour {

	/// <summary>
	/// Set Customization Mode. This will enable / disable controlling the car, and enable / disable orbit camera mode.
	/// </summary>
	public static void SetCustomizationMode(RCC_CarControllerV3 car, bool state){

		if (!car) {
			Debug.LogError ("Player Car is not selected for customization!");
			return;
		}

		if (state) {

			car.canControl = false;

			if (GameObject.FindObjectOfType<RCC_Camera> ()) {
				RCC_Camera cam = GameObject.FindObjectOfType<RCC_Camera> ();
				cam.ChangeCamera (RCC_Camera.CameraMode.ORBIT);
				cam.useOnlyWhenHold = true;
			}

		} else {

			SetSmokeParticle (car, false);
			SetExhaustFlame (car, false);
			car.canControl = true;

			if (GameObject.FindObjectOfType<RCC_Camera> ()) {
				GameObject.FindObjectOfType<RCC_Camera> ().ChangeCamera (RCC_Camera.CameraMode.TPS);
				GameObject.FindObjectOfType<RCC_Camera> ().useOnlyWhenHold = false;
			}

		}

	}

	/// <summary>
	///	 Updates RCC car while car is inactive.
	/// </summary>
	public static void UpdateRCC (RCC_CarControllerV3 car) {

		car.sleepingRigid = false;

	}

	/// <summary>
	///	 Enable / Disable Smoke Particles. You can use it for previewing current wheel smokes.
	/// </summary>
	public static void SetSmokeParticle (RCC_CarControllerV3 car, bool state) {

		car.PreviewSmokeParticle (state);

	}

	/// <summary>
	/// Set Smoke Color.
	/// </summary>
	public static void SetSmokeColor (RCC_CarControllerV3 car, int indexOfGroundMaterial, Color color) {

		RCC_WheelCollider[] wheels = car.GetComponentsInChildren<RCC_WheelCollider> ();

		foreach(RCC_WheelCollider wheel in wheels){

			for (int i = 0; i < wheel.allWheelParticles.Count; i++) {
				wheel.allWheelParticles [i].startColor = color;
			}

		}

	}

	/// <summary>
	/// Set Headlights Color.
	/// </summary>
	public static void SetHeadlightsColor (RCC_CarControllerV3 car, Color color) {

		RCC_Light[] lights = car.GetComponentsInChildren<RCC_Light> ();
		car.lowBeamHeadLightsOn = true;

		foreach(RCC_Light l in lights){

			if(l.lightType == RCC_Light.LightType.HeadLight)
				l.GetComponent<Light>().color = color;

		}

	}

	/// <summary>
	/// Enable / Disable Exhaust Flame Particles.
	/// </summary>
	public static void SetExhaustFlame (RCC_CarControllerV3 car, bool state) {

		RCC_Exhaust[] exhausts = car.GetComponentsInChildren<RCC_Exhaust> ();

		foreach (RCC_Exhaust exhaust in exhausts) {
			exhaust.previewFlames = state;
		}

	}

	/// <summary>
	/// Set Front Wheel Cambers.
	/// </summary>
	public static void SetFrontCambers(RCC_CarControllerV3 car, float camberAngle){

		RCC_WheelCollider[] wc = car.GetComponentsInChildren<RCC_WheelCollider> ();

		foreach (RCC_WheelCollider w in wc) {
			if (w == car.FrontLeftWheelCollider || w == car.FrontRightWheelCollider)
				w.camber = camberAngle;
		}

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Rear Wheel Cambers.
	/// </summary>
	public static void SetRearCambers(RCC_CarControllerV3 car, float camberAngle){

		RCC_WheelCollider[] wc = car.GetComponentsInChildren<RCC_WheelCollider> ();

		foreach (RCC_WheelCollider w in wc) {
			if (w != car.FrontLeftWheelCollider && w != car.FrontRightWheelCollider)
				w.camber = camberAngle;
		}

		UpdateRCC (car);

	}

	/// <summary>
	/// Change Wheel Models. You can find your wheel array in Tools --> BCG --> RCC --> Configure Changable Wheels.
	/// </summary>
	public static void ChangeWheels(RCC_CarControllerV3 car, GameObject wheel){

		for (int i = 0; i < car.allWheelColliders.Length; i++) {

			if (car.allWheelColliders [i].wheelModel.GetComponent<MeshRenderer> ()) 
				car.allWheelColliders [i].wheelModel.GetComponent<MeshRenderer> ().enabled = false;

			foreach (Transform t in car.allWheelColliders [i].wheelModel.GetComponentInChildren<Transform> ()) {
				t.gameObject.SetActive (false);
			}

			GameObject newWheel = (GameObject)Instantiate (wheel, car.allWheelColliders[i].wheelModel.position, car.allWheelColliders[i].wheelModel.rotation, car.allWheelColliders[i].wheelModel);

			if (car.allWheelColliders [i].wheelModel.localPosition.x > 0f)
				newWheel.transform.localScale = new Vector3 (newWheel.transform.localScale.x * -1f, newWheel.transform.localScale.y, newWheel.transform.localScale.z);

		}

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Front Suspension targetPositions. It changes targetPosition of the front WheelColliders.
	/// </summary>
	public static void SetFrontSuspensionsTargetPos(RCC_CarControllerV3 car, float targetPosition){

		JointSpring spring1 = car.FrontLeftWheelCollider.wheelCollider.suspensionSpring;
		spring1.targetPosition = 1 - targetPosition;

		car.FrontLeftWheelCollider.wheelCollider.suspensionSpring = spring1;

		JointSpring spring2 = car.FrontRightWheelCollider.wheelCollider.suspensionSpring;
		spring2.targetPosition = 1 - targetPosition;

		car.FrontRightWheelCollider.wheelCollider.suspensionSpring = spring2;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Rear Suspension targetPositions. It changes targetPosition of the rear WheelColliders.
	/// </summary>
	public static void SetRearSuspensionsTargetPos(RCC_CarControllerV3 car, float targetPosition){

		JointSpring spring1 = car.RearLeftWheelCollider.wheelCollider.suspensionSpring;
		spring1.targetPosition = 1 - targetPosition;

		car.RearLeftWheelCollider.wheelCollider.suspensionSpring = spring1;

		JointSpring spring2 = car.RearRightWheelCollider.wheelCollider.suspensionSpring;
		spring2.targetPosition = 1 - targetPosition;

		car.RearRightWheelCollider.wheelCollider.suspensionSpring = spring2;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Front Suspension Distances.
	/// </summary>
	public static void SetFrontSuspensionsDistances(RCC_CarControllerV3 car, float distance){

		if (distance <= 0)
			distance = .05f;

		car.FrontLeftWheelCollider.wheelCollider.suspensionDistance = distance;
		car.FrontRightWheelCollider.wheelCollider.suspensionDistance = distance;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Rear Suspension Distances.
	/// </summary>
	public static void SetRearSuspensionsDistances(RCC_CarControllerV3 car, float distance){

		if (distance <= 0)
			distance = .05f;

		car.RearLeftWheelCollider.wheelCollider.suspensionDistance = distance;
		car.RearRightWheelCollider.wheelCollider.suspensionDistance = distance;

		if (car.ExtraRearWheelsCollider != null && car.ExtraRearWheelsCollider.Length > 0) {
			foreach (RCC_WheelCollider wc in car.ExtraRearWheelsCollider)
				wc.wheelCollider.suspensionDistance = distance;
		}

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Drivetrain Mode.
	/// </summary>
	public static void SetDrivetrainMode(RCC_CarControllerV3 car, RCC_CarControllerV3.WheelType mode){

		car._wheelTypeChoise = mode;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Gear Shifting Threshold. Automatic gear will shift up at earlier rpm on lower values. Automatic gear will shift up at later rpm on higher values. 
	/// </summary>
	public static void SetGearShiftingThreshold(RCC_CarControllerV3 car, float targetValue){

		car.gearShiftingThreshold = targetValue;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Clutch Threshold. Automatic gear will shift up at earlier rpm on lower values. Automatic gear will shift up at later rpm on higher values. 
	/// </summary>
	public static void SetClutchThreshold(RCC_CarControllerV3 car, float targetValue){

		car.clutchInertia = targetValue;

		UpdateRCC (car);

	}

	/// <summary>
	/// Enable / Disable Steering Sensitivity. Useful for avoid fast steering reactions on higher speeds.
	/// </summary>
	public static void SetSteeringSensitivity(RCC_CarControllerV3 car, bool state){

		car.steerAngleSensitivityAdjuster = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// Enable / Disable Counter Steering while car is drifting. Useful for avoid spinning.
	/// </summary>
	public static void SetCounterSteering(RCC_CarControllerV3 car, bool state){

		car.applyCounterSteering = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// 
	/// </summary>
	public static void SetNOS(RCC_CarControllerV3 car, bool state){

		car.useNOS = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// 
	/// </summary>
	public static void SetTurbo(RCC_CarControllerV3 car, bool state){

		car.useTurbo = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// 
	/// </summary>
	public static void SetUseExhaustFlame(RCC_CarControllerV3 car, bool state){

		car.useExhaustFlame = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// 
	/// </summary>
	public static void SetRevLimiter(RCC_CarControllerV3 car, bool state){

		car.useRevLimiter = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// 
	/// </summary>
	public static void SetClutchMargin(RCC_CarControllerV3 car, bool state){

		car.useClutchMarginAtFirstGear = state;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Front Suspension Spring Force.
	/// </summary>
	public static void SetFrontSuspensionsSpringForce(RCC_CarControllerV3 car, float targetValue){

		JointSpring spring = car.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.spring = targetValue;
		car.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		car.FrontRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Rear Suspension Spring Force.
	/// </summary>
	public static void SetRearSuspensionsSpringForce(RCC_CarControllerV3 car, float targetValue){

		JointSpring spring = car.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.spring = targetValue;
		car.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		car.RearRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Front Suspension Spring Damper.
	/// </summary>
	public static void SetFrontSuspensionsSpringDamper(RCC_CarControllerV3 car, float targetValue){

		JointSpring spring = car.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.damper = targetValue;
		car.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		car.FrontRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Rear Suspension Spring Damper.
	/// </summary>
	public static void SetRearSuspensionsSpringDamper(RCC_CarControllerV3 car, float targetValue){

		JointSpring spring = car.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.damper = targetValue;
		car.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		car.RearRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Maximum Speed of the car.
	/// </summary>
	public static void SetMaximumSpeed(RCC_CarControllerV3 car, float targetValue){

		car.maxspeed = Mathf.Clamp(targetValue, 10f, 300f);

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Maximum Engine Torque of the car.
	/// </summary>
	public static void SetMaximumTorque(RCC_CarControllerV3 car, float targetValue){

		car.engineTorque = Mathf.Clamp(targetValue, 500f, 50000f);

		UpdateRCC (car);

	}

	/// <summary>
	/// Set Maximum Brake of the car.
	/// </summary>
	public static void SetMaximumBrake(RCC_CarControllerV3 car, float targetValue){

		car.brakeTorque = Mathf.Clamp(targetValue, 0f, 50000f);

		UpdateRCC (car);

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void RepairCar(RCC_CarControllerV3 car){

		car.repairNow = true;

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetESP(RCC_CarControllerV3 car, bool state){

		car.ESP = state;

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetABS(RCC_CarControllerV3 car, bool state){

		car.ABS = state;

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetTCS(RCC_CarControllerV3 car, bool state){

		car.TCS = state;

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetSH(RCC_CarControllerV3 car, bool state){

		car.steeringHelper = state;

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetSHStrength(RCC_CarControllerV3 car, float value){

		car.steeringHelper = true;
		car.steerHelperLinearVelStrength = value;
		car.steerHelperAngularVelStrength = value;

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetCameraOrbitOnDrag(BaseEventData data){

		RCC_Camera cam = GameObject.FindObjectOfType<RCC_Camera> ();

		if (!cam)
			return;

		if(cam.cameraMode != RCC_Camera.CameraMode.ORBIT)
			cam.ChangeCamera (RCC_Camera.CameraMode.ORBIT);

		cam.useOnlyWhenHold = true;

		cam.OnDrag (data);

	}

	/// <summary>
	/// Repair Car.
	/// </summary>
	public static void SetTransmission(bool automatic){

		RCC_Settings.Instance.useAutomaticGear = automatic;

	}




	/// <summary>
	/// Save all stats with PlayerPrefs.
	/// </summary>
	public static void SaveStats(RCC_CarControllerV3 car){

		PlayerPrefs.SetFloat(car.transform.name + "_FrontCamber", car.FrontLeftWheelCollider.camber);
		PlayerPrefs.SetFloat(car.transform.name + "_RearCamber", car.RearLeftWheelCollider.camber);

		PlayerPrefs.SetFloat(car.transform.name + "_FrontSuspensionsDistance", car.FrontLeftWheelCollider.wheelCollider.suspensionDistance);
		PlayerPrefs.SetFloat(car.transform.name + "_RearSuspensionsDistance", car.RearLeftWheelCollider.wheelCollider.suspensionDistance);

		PlayerPrefs.SetFloat(car.transform.name + "_FrontSuspensionsSpring", car.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring);
		PlayerPrefs.SetFloat(car.transform.name + "_RearSuspensionsSpring", car.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring);

		PlayerPrefs.SetFloat(car.transform.name + "_FrontSuspensionsDamper", car.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper);
		PlayerPrefs.SetFloat(car.transform.name + "_RearSuspensionsDamper", car.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper);

		PlayerPrefs.SetFloat(car.transform.name + "_MaximumSpeed", car.maxspeed);
		PlayerPrefs.SetFloat(car.transform.name + "_MaximumBrake", car.brakeTorque);
		PlayerPrefs.SetFloat(car.transform.name + "_MaximumTorque", car.engineTorque);

		PlayerPrefs.SetString(car.transform.name + "_DrivetrainMode", car._wheelTypeChoise.ToString());

		PlayerPrefs.SetFloat(car.transform.name + "_GearShiftingThreshold", car.gearShiftingThreshold);
		PlayerPrefs.SetFloat(car.transform.name + "_ClutchingThreshold", car.clutchInertia);

		PlayerPrefsX.SetBool(car.transform.name + "_CounterSteering", car.applyCounterSteering);

		foreach(RCC_Light _light in car.GetComponentsInChildren<RCC_Light>()){
			if (_light.lightType == RCC_Light.LightType.HeadLight) {
				PlayerPrefsX.SetColor(car.transform.name + "_HeadlightsColor", _light.GetComponentInChildren<Light>().color);
				break;
			}
		}

		PlayerPrefsX.SetColor(car.transform.name + "_WheelsSmokeColor", car.RearLeftWheelCollider.allWheelParticles[0].startColor);

		PlayerPrefsX.SetBool(car.transform.name + "_ABS", car.ABS);
		PlayerPrefsX.SetBool(car.transform.name + "_ESP", car.ESP);
		PlayerPrefsX.SetBool(car.transform.name + "_TCS", car.TCS);
		PlayerPrefsX.SetBool(car.transform.name + "_SH", car.steeringHelper);

		PlayerPrefsX.SetBool(car.transform.name + "NOS", car.useNOS);
		PlayerPrefsX.SetBool(car.transform.name + "Turbo", car.useTurbo);
		PlayerPrefsX.SetBool(car.transform.name + "ExhaustFlame", car.useExhaustFlame);
		PlayerPrefsX.SetBool(car.transform.name + "SteeringSensitivity", car.steerAngleSensitivityAdjuster);
		PlayerPrefsX.SetBool(car.transform.name + "RevLimiter", car.useRevLimiter);
		PlayerPrefsX.SetBool(car.transform.name + "ClutchMargin", car.useClutchMarginAtFirstGear);

	}

	/// <summary>
	/// Load all stats with PlayerPrefs.
	/// </summary>
	public static void LoadStats(RCC_CarControllerV3 car){

		SetFrontCambers (car, PlayerPrefs.GetFloat(car.transform.name + "_FrontCamber", car.FrontLeftWheelCollider.camber));
		SetRearCambers (car, PlayerPrefs.GetFloat(car.transform.name + "_RearCamber", car.RearLeftWheelCollider.camber));

		SetFrontSuspensionsDistances (car, PlayerPrefs.GetFloat(car.transform.name + "_FrontSuspensionsDistance", car.FrontLeftWheelCollider.wheelCollider.suspensionDistance));
		SetRearSuspensionsDistances (car, PlayerPrefs.GetFloat(car.transform.name + "_RearSuspensionsDistance", car.RearLeftWheelCollider.wheelCollider.suspensionDistance));

		SetFrontSuspensionsSpringForce (car, PlayerPrefs.GetFloat(car.transform.name + "_FrontSuspensionsSpring", car.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring));
		SetRearSuspensionsSpringForce (car, PlayerPrefs.GetFloat(car.transform.name + "_RearSuspensionsSpring", car.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring));

		SetFrontSuspensionsSpringDamper (car, PlayerPrefs.GetFloat(car.transform.name + "_FrontSuspensionsDamper", car.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper));
		SetRearSuspensionsSpringDamper (car, PlayerPrefs.GetFloat(car.transform.name + "_RearSuspensionsDamper", car.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper));

		SetMaximumSpeed (car, PlayerPrefs.GetFloat(car.transform.name + "_MaximumSpeed", car.maxspeed));
		SetMaximumBrake (car, PlayerPrefs.GetFloat(car.transform.name + "_MaximumBrake", car.brakeTorque));
		SetMaximumTorque (car, PlayerPrefs.GetFloat(car.transform.name + "_MaximumTorque", car.engineTorque));

		string drvtrn = PlayerPrefs.GetString(car.transform.name + "_DrivetrainMode", car._wheelTypeChoise.ToString());

		switch (drvtrn) {

		case "FWD":
			car._wheelTypeChoise = RCC_CarControllerV3.WheelType.FWD;
			break;

		case "RWD":
			car._wheelTypeChoise = RCC_CarControllerV3.WheelType.RWD;
			break;

		case "AWD":
			car._wheelTypeChoise = RCC_CarControllerV3.WheelType.AWD;
			break;

		}

		SetGearShiftingThreshold (car, PlayerPrefs.GetFloat(car.transform.name + "_GearShiftingThreshold", car.gearShiftingThreshold));
		SetClutchThreshold(car, PlayerPrefs.GetFloat(car.transform.name + "_ClutchingThreshold", car.clutchInertia));

		SetCounterSteering (car, PlayerPrefsX.GetBool(car.transform.name + "_CounterSteering", car.applyCounterSteering));

		SetABS (car, PlayerPrefsX.GetBool(car.transform.name + "_ABS", car.ABS));
		SetESP (car, PlayerPrefsX.GetBool(car.transform.name + "_ESP", car.ESP));
		SetTCS (car, PlayerPrefsX.GetBool(car.transform.name + "_TCS", car.TCS));
		SetSH (car, PlayerPrefsX.GetBool(car.transform.name + "_SH", car.steeringHelper));

		SetNOS (car, PlayerPrefsX.GetBool(car.transform.name + "NOS", car.useNOS));
		SetTurbo (car, PlayerPrefsX.GetBool(car.transform.name + "Turbo", car.useTurbo));
		SetUseExhaustFlame (car, PlayerPrefsX.GetBool(car.transform.name + "ExhaustFlame", car.useExhaustFlame));
		SetSteeringSensitivity (car, PlayerPrefsX.GetBool(car.transform.name + "SteeringSensitivity", car.steerAngleSensitivityAdjuster));
		SetRevLimiter (car, PlayerPrefsX.GetBool(car.transform.name + "RevLimiter", car.useRevLimiter));
		SetClutchMargin (car, PlayerPrefsX.GetBool(car.transform.name + "ClutchMargin", car.useClutchMarginAtFirstGear));

		if(PlayerPrefs.HasKey(car.transform.name + "_WheelsSmokeColor"))
			SetSmokeColor (car, 0, PlayerPrefsX.GetColor(car.transform.name + "_WheelsSmokeColor"));

		if(PlayerPrefs.HasKey(car.transform.name + "_HeadlightsColor"))
			SetHeadlightsColor (car, PlayerPrefsX.GetColor(car.transform.name + "_HeadlightsColor"));

		UpdateRCC (car);

	}

	/// <summary>
	/// Resets all stats with PlayerPrefs.
	/// </summary>
	public static void ResetStats(RCC_CarControllerV3 car, RCC_CarControllerV3 defaultCar){

		SetFrontCambers (car, defaultCar.FrontLeftWheelCollider.camber);
		SetRearCambers (car, defaultCar.RearLeftWheelCollider.camber);

		SetFrontSuspensionsDistances (car, defaultCar.FrontLeftWheelCollider.wheelCollider.suspensionDistance);
		SetRearSuspensionsDistances (car, defaultCar.RearLeftWheelCollider.wheelCollider.suspensionDistance);

		SetFrontSuspensionsSpringForce (car, defaultCar.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring);
		SetRearSuspensionsSpringForce (car, defaultCar.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring);

		SetFrontSuspensionsSpringDamper (car, defaultCar.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper);
		SetRearSuspensionsSpringDamper (car, defaultCar.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper);

		SetMaximumSpeed (car, defaultCar.maxspeed);
		SetMaximumBrake (car, defaultCar.brakeTorque);
		SetMaximumTorque (car, defaultCar.engineTorque);

		string drvtrn = defaultCar._wheelTypeChoise.ToString();

		switch (drvtrn) {

		case "FWD":
			car._wheelTypeChoise = RCC_CarControllerV3.WheelType.FWD;
			break;

		case "RWD":
			car._wheelTypeChoise = RCC_CarControllerV3.WheelType.RWD;
			break;

		case "AWD":
			car._wheelTypeChoise = RCC_CarControllerV3.WheelType.AWD;
			break;

		}

		SetGearShiftingThreshold (car, defaultCar.gearShiftingThreshold);
		SetClutchThreshold(car, defaultCar.clutchInertia);

		SetCounterSteering (car, defaultCar.applyCounterSteering);

		SetABS (car, defaultCar.ABS);
		SetESP (car, defaultCar.ESP);
		SetTCS (car, defaultCar.TCS);
		SetSH (car, defaultCar.steeringHelper);

		SetNOS (car, defaultCar.useNOS);
		SetTurbo (car, defaultCar.useTurbo);
		SetUseExhaustFlame (car, defaultCar.useExhaustFlame);
		SetSteeringSensitivity (car, defaultCar.steerAngleSensitivityAdjuster);
		SetRevLimiter (car, defaultCar.useRevLimiter);
		SetClutchMargin (car, defaultCar.useClutchMarginAtFirstGear);

		SetSmokeColor (car, 0, Color.white);
		SetHeadlightsColor (car, Color.white);

		SaveStats (car);
		UpdateRCC (car);

	}

}
