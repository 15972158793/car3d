//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------


using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Based on Unity's WheelCollider. It just modifies few curves, settings in order to get stable and realistic physics.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Main/Wheel Collider")]
[RequireComponent (typeof(WheelCollider))]
public class RCC_WheelCollider : MonoBehaviour {

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

	// Getting an Instance of Ground Materials.
	#region RCC_GroundMaterials Instance

	private RCC_GroundMaterials RCCGroundMaterialsInstance;
	private RCC_GroundMaterials RCCGroundMaterials {
		get {
			if (RCCGroundMaterialsInstance == null) {
				RCCGroundMaterialsInstance = RCC_GroundMaterials.Instance;
			}
			return RCCGroundMaterialsInstance;
		}
	}

	#endregion

	private WheelCollider _wheelCollider;
	public WheelCollider wheelCollider{
		get{
			if(_wheelCollider == null)
				_wheelCollider = GetComponent<WheelCollider>();
			return _wheelCollider;
		}set{
			_wheelCollider = value;
		}
	}
	
	private RCC_CarControllerV3 carController;
	private Rigidbody rigid;

	private List <RCC_WheelCollider> allWheelColliders = new List<RCC_WheelCollider>() ;		// All other wheelcolliders attached to this car.
	public Transform wheelModel;		// Wheel model.

	private float wheelRotation = 0f;		// Wheel model rotation based on WheelCollider rpm. 
	public float camber = 0f;		// Camber angle.

	internal float wheelRPMToSpeed = 0f;		// Wheel RPM to Speed.

	private RCC_GroundMaterials physicsMaterials{get{return RCCGroundMaterials;}}		// Instance of Configurable Ground Materials.
	private RCC_GroundMaterials.GroundMaterialFrictions[] physicsFrictions{get{return RCCGroundMaterials.frictions;}}

	private RCC_Skidmarks skidmarks;		// Main Skidmarks Manager class.
	private float startSlipValue = .25f;		// Draw skidmarks when forward or sideways slip is bigger than this value.
	private int lastSkidmark = -1;

	private float wheelSlipAmountForward = 0f;		// Forward slip.
	private float wheelSlipAmountSideways = 0f;		// Sideways slip.
	internal float totalSlip = 0f;		// Total amount of forward and sideways slips.

	private float orgForwardStiffness = 1f;		// Default forward stiffness.
	private float orgSidewaysStiffness = 1f;		// Default sideways stiffness.

	//WheelFriction Curves and Stiffness.
	public WheelFrictionCurve forwardFrictionCurve;
	public WheelFrictionCurve sidewaysFrictionCurve;

	private AudioSource audioSource;		// Audiosource for tire skid SFX.
	private AudioClip audioClip;		// Audioclip for tire skid SFX.

	// List for all particle systems.
	internal List<ParticleSystem> allWheelParticles = new List<ParticleSystem>();
	internal ParticleSystem.EmissionModule emission;

	internal float tractionHelpedSidewaysStiffness = 1f;

	private float minForwardStiffness = .75f;
	private float maxForwardStiffness  = 1f;

	private float minSidewaysStiffness = .75f;
	private float maxSidewaysStiffness = 1f;
	
	void Awake (){
		
		carController = GetComponentInParent<RCC_CarControllerV3>();
		rigid = carController.GetComponent<Rigidbody> ();

		// Are we going to use skidmarks? If we do, get or create SkidmarksManager on scene.
		if (!RCCSettings.dontUseSkidmarks) {
			if (FindObjectOfType (typeof(RCC_Skidmarks))) {
				skidmarks = FindObjectOfType (typeof(RCC_Skidmarks)) as RCC_Skidmarks;
			} else {
				skidmarks = (RCC_Skidmarks)Instantiate (RCCSettings.skidmarksManager, Vector3.zero, Quaternion.identity);
			}
		}

		// Increasing WheelCollider mass for avoiding unstable behavior. Only in Unity 5.
		wheelCollider.mass = rigid.mass / 15f;

		// Getting friction curves.
		forwardFrictionCurve = wheelCollider.forwardFriction;
		sidewaysFrictionCurve = wheelCollider.sidewaysFriction;

		// Proper settings for selected behavior type.
		switch(RCCSettings.behaviorType){

		case RCC_Settings.BehaviorType.SemiArcade:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 2f, 2f, 2f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .25f, 2f, 2f, 2f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .35f, 1f);
			break;

		case RCC_Settings.BehaviorType.Drift:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .25f, 1f, .8f, .5f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .4f, 1f, .5f, .75f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .1f, 1f);
			if(carController._wheelTypeChoise == RCC_CarControllerV3.WheelType.FWD){
				Debug.LogError("Current behavior mode is ''Drift'', but your vehicle named " + carController.name + " was FWD. You have to use RWD, AWD, or BIASED to rear wheels. Setting it to *RWD* now. ");
				carController._wheelTypeChoise = RCC_CarControllerV3.WheelType.RWD;
			}
			break;

		case RCC_Settings.BehaviorType.Fun:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 2f, 2f, 2f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .25f, 2f, 2f, 2f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .75f, 2f);
			break;

		case RCC_Settings.BehaviorType.Racing:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 1f, .8f, .75f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .3f, 1f, .25f, .75f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .25f, 1f);
			break;

		case RCC_Settings.BehaviorType.Simulator:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 1f, .8f, .75f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .25f, 1f, .5f, .75f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .1f, 1f);
			break;

		}

		// Getting default stiffness.
		orgForwardStiffness = forwardFrictionCurve.stiffness;
		orgSidewaysStiffness = sidewaysFrictionCurve.stiffness;

		// Assigning new frictons if one of the behavior preset selected above.
		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

		// Creating audiosource for skid SFX.
		if(RCCSettings.useSharedAudioSources){
			if(!carController.transform.Find("All Audio Sources/Skid Sound AudioSource"))
				audioSource = RCC_CreateAudioSource.NewAudioSource(carController.gameObject, "Skid Sound AudioSource", 5, 50, 0, audioClip, true, true, false);
			else
				audioSource = carController.transform.Find("All Audio Sources/Skid Sound AudioSource").GetComponent<AudioSource>();
		}else{
			audioSource = RCC_CreateAudioSource.NewAudioSource(carController.gameObject, "Skid Sound AudioSource", 5, 50, 0, audioClip, true, true, false);
			audioSource.transform.position = transform.position;
		}

		// Creating all ground particles, and adding them to list.
		if (!RCCSettings.dontUseAnyParticleEffects) {

			for (int i = 0; i < RCCGroundMaterials.frictions.Length; i++) {

				GameObject ps = (GameObject)Instantiate (RCCGroundMaterials.frictions [i].groundParticles, transform.position, transform.rotation) as GameObject;
				emission = ps.GetComponent<ParticleSystem> ().emission;
				emission.enabled = false;
				ps.transform.SetParent (transform, false);
				ps.transform.localPosition = Vector3.zero;
				ps.transform.localRotation = Quaternion.identity;
				allWheelParticles.Add (ps.GetComponent<ParticleSystem> ());

			}

		}
			
	}

	void Start(){

		// Getting all WheelColliders attached to this car (Except this).
		allWheelColliders = carController.allWheelColliders.ToList();
		allWheelColliders.Remove(this);

	}

	// Setting a new friction to WheelCollider.
	private WheelFrictionCurve SetFrictionCurves(WheelFrictionCurve curve, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue){

		WheelFrictionCurve newCurve = curve;

		newCurve.extremumSlip = extremumSlip;
		newCurve.extremumValue = extremumValue;
		newCurve.asymptoteSlip = asymptoteSlip;
		newCurve.asymptoteValue = asymptoteValue;

		return newCurve;

	}

	void Update(){

		// Return if main car controller script is disabled.
		if (!carController.enabled)
			return;

		// Only runs when car is active. Raycasts are used for WheelAlign().
		if(!carController.sleepingRigid){
			
			WheelAlign();
			WheelCamber();

		}

		#region Motor Torque, TCS.

		//Applying WheelCollider Motor Torques Depends On Wheel Type Choice.
		switch(carController._wheelTypeChoise){

		case RCC_CarControllerV3.WheelType.FWD:
			if(this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyMotorTorque(carController.engineTorque);
			break;
		case RCC_CarControllerV3.WheelType.RWD:
			if(this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyMotorTorque(carController.engineTorque);
			break;
		case RCC_CarControllerV3.WheelType.AWD:
			ApplyMotorTorque(carController.engineTorque / 2f);
			break;
		case RCC_CarControllerV3.WheelType.BIASED:
			if(this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyMotorTorque((carController.engineTorque * (100 - carController.biasedWheelTorque)) / 100f);
			if(this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyMotorTorque((carController.engineTorque * carController.biasedWheelTorque) / 100f);
			break;

		}

		if(carController.ExtraRearWheelsCollider.Length > 0 && carController.applyEngineTorqueToExtraRearWheelColliders){

			for(int i = 0; i < carController.ExtraRearWheelsCollider.Length; i++){

				if(this == carController.ExtraRearWheelsCollider[i])
					ApplyMotorTorque(carController.engineTorque);

			}

		}

		#endregion

		#region Steering.

		// Apply Steering if this wheel is one of the front wheels.
		if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider) {
			ApplySteering ();
		}

		#endregion

		#region Braking, ABS.

		// Apply Handbrake if this wheel is one of the rear wheels.
		if(carController.handbrakeInput > .1f){

			if(this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyBrakeTorque((carController.brakeTorque * 1.5f) * carController.handbrakeInput);

		}else{

			// Apply Braking to all wheels.
			if(this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyBrakeTorque(carController.brakeTorque * (Mathf.Clamp(carController._brakeInput, 0, 1)));
			else
				ApplyBrakeTorque(carController.brakeTorque * (Mathf.Clamp(carController._brakeInput, 0, 1) / 2f));

		}

		#endregion

		#region ESP.

		// ESP System. All wheels have individual brakes. In case of loosing control of the car, corresponding wheel will brake for gaining the control again.
		if (carController.ESP) {

			if(carController.underSteering){
				
				if(this == carController.RearLeftWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.frontSlip, 0f, Mathf.Infinity));
				
				if(this == carController.RearRightWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.frontSlip, 0f, Mathf.Infinity));
				
			}

			if(carController.overSteering){

				if(this == carController.FrontLeftWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.rearSlip, 0f, Mathf.Infinity));

				if(this == carController.FrontRightWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.rearSlip, 0f, Mathf.Infinity));

			}

		}

		#endregion

	}
	
	void  FixedUpdate (){

		// Return if main car controller script is disabled.
		if (!carController.enabled)
			return;

		wheelRPMToSpeed = (((wheelCollider.rpm * wheelCollider.radius) / 2.9f)) * rigid.transform.lossyScale.y;

		SkidMarks();
		Frictions();
		Audio();

	}

	// Aligning wheel model position and rotation.
	public void WheelAlign (){

		// Return if no wheel model selected.
		if(!wheelModel){
			Debug.LogError(transform.name + " wheel of the " + carController.transform.name + " is missing wheel model. This wheel is disabled");
			enabled = false;
			return;
		}

		// First, we are getting groundhit data.
		RaycastHit hit;
		WheelHit CorrespondingGroundHit;

		// Taking WheelCollider center position.
		Vector3 ColliderCenterPoint = wheelCollider.transform.TransformPoint(wheelCollider.center);
		wheelCollider.GetGroundHit(out CorrespondingGroundHit);

		// Here we are raycasting to downwards.
		if(Physics.Raycast(ColliderCenterPoint, -wheelCollider.transform.up, out hit, (wheelCollider.suspensionDistance + wheelCollider.radius) * transform.localScale.y) && !hit.transform.IsChildOf(carController.transform) && !hit.collider.isTrigger){
			// Assigning position of the wheel if we have hit.
			wheelModel.transform.position = hit.point + (wheelCollider.transform.up * wheelCollider.radius) * transform.localScale.y;
			float extension = (-wheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - wheelCollider.radius) / wheelCollider.suspensionDistance;
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + wheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - wheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip * 2f, Color.green);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - wheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip * 2f, Color.red);
		}else{
			// Assigning position of the wheel to default position if we don't have hit.
			wheelModel.transform.position = ColliderCenterPoint - (wheelCollider.transform.up * wheelCollider.suspensionDistance) * transform.localScale.y;
		}

		// X axis rotation of the wheel.
		wheelRotation += wheelCollider.rpm * 6 * Time.deltaTime;

		// Assigning rotation of the wheel (X and Y axises).
		wheelModel.transform.rotation = wheelCollider.transform.rotation * Quaternion.Euler(wheelRotation, wheelCollider.steerAngle, wheelCollider.transform.rotation.z);

	}

	// Rotating Z axis of the wheel for camber.
	public void WheelCamber (){

		Vector3 wheelLocalEuler;

		if(wheelCollider.transform.localPosition.x < 0)
			wheelLocalEuler = new Vector3(wheelCollider.transform.localEulerAngles.x, wheelCollider.transform.localEulerAngles.y, (-camber));
		else
			wheelLocalEuler = new Vector3(wheelCollider.transform.localEulerAngles.x, wheelCollider.transform.localEulerAngles.y, (camber));

		Quaternion wheelCamber = Quaternion.Euler(wheelLocalEuler);
		wheelCollider.transform.localRotation = wheelCamber;

	}

	// Creating skidmarks.
	void SkidMarks(){

		// First, we are getting groundhit data.
		WheelHit GroundHit;
		wheelCollider.GetGroundHit(out GroundHit);

		// Forward, sideways, and total slips.
		wheelSlipAmountSideways = Mathf.Abs(GroundHit.sidewaysSlip);
		wheelSlipAmountForward = Mathf.Abs(GroundHit.forwardSlip);
		totalSlip = wheelSlipAmountSideways + (wheelSlipAmountForward / 2f);

		// If scene has skidmarks manager...
		if(skidmarks){

			// If slips are bigger than target value...
			if (wheelSlipAmountSideways > startSlipValue || wheelSlipAmountForward > startSlipValue * 2f){

				Vector3 skidPoint = GroundHit.point + 2f * (rigid.velocity) * Time.deltaTime;

				if(rigid.velocity.magnitude > 1f){
					lastSkidmark = skidmarks.AddSkidMark(skidPoint, GroundHit.normal, (wheelSlipAmountSideways / 2f) + (wheelSlipAmountForward / 2f), lastSkidmark);
				}else{
					lastSkidmark = -1;
				}

			}else{
				
				lastSkidmark = -1;

			}

		}

	}

	// Setting ground frictions to wheel frictions.
	void Frictions(){

		// First, we are getting groundhit data.
		WheelHit GroundHit;
		wheelCollider.GetGroundHit(out GroundHit);

		// Contacted any physic material in Configurable Ground Materials yet?
		bool contacted = false;

		for (int i = 0; i < physicsFrictions.Length; i++) {

			if(GroundHit.point != Vector3.zero && GroundHit.collider.sharedMaterial == physicsFrictions[i].groundMaterial){

				contacted = true;

				// Setting wheel stiffness to ground physic material stiffness.
				forwardFrictionCurve.stiffness = physicsFrictions[i].forwardStiffness;
				sidewaysFrictionCurve.stiffness = (physicsFrictions[i].sidewaysStiffness * tractionHelpedSidewaysStiffness);

				// If drift mode is selected, apply specific frictions.
				if(RCCSettings.behaviorType == RCC_Settings.BehaviorType.Drift){
					Drift();
				}

				// Setting new friction curves to wheels.
				wheelCollider.forwardFriction = forwardFrictionCurve;
				wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

				// Also damp too.
				wheelCollider.wheelDampingRate = physicsFrictions[i].damp;

				// If dontUseAnyParticleEffects bool is not selected in RCC_Settings, set emission to ground physic material smoke.
				if (!RCCSettings.dontUseAnyParticleEffects) 
					emission = allWheelParticles[i].emission;

				// Set audioclip to ground physic material sound.
				audioClip = physicsFrictions[i].groundSound;

				// If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
				if (!RCCSettings.dontUseAnyParticleEffects) {
					
					if (wheelSlipAmountSideways > physicsFrictions [i].slip || wheelSlipAmountForward > physicsFrictions [i].slip) {
						emission.enabled = true;
					} else {
						emission.enabled = false;
					}

				}

			}

		}

		// If ground pyhsic material is not one of the ground material in Configurable Ground Materials, check if we are on terrain collider...
		if(!contacted && physicsMaterials.useTerrainSplatMapForGroundFrictions){

			for (int k = 0; k < physicsMaterials.terrainSplatMapIndex.Length; k++) {

				// If current ground is terrain collider...
				if(GroundHit.point != Vector3.zero && GroundHit.collider.sharedMaterial == physicsMaterials.terrainPhysicMaterial){

					// Getting current exact position by splatmap of the terrain.
					if(TerrainSurface.GetTextureMix(transform.position) != null && TerrainSurface.GetTextureMix(transform.position)[k] > .5f){

						contacted = true;

						// Setting wheel stiffness to ground physic material stiffness.
						forwardFrictionCurve.stiffness = physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].forwardStiffness;
						sidewaysFrictionCurve.stiffness = (physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].sidewaysStiffness * tractionHelpedSidewaysStiffness);

						// If drift mode is selected, apply specific frictions.
						if(RCCSettings.behaviorType == RCC_Settings.BehaviorType.Drift){
							Drift();
						}

						// Setting new friction curves to wheels.
						wheelCollider.forwardFriction = forwardFrictionCurve;
						wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

						// Also damp too.
						wheelCollider.wheelDampingRate = physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].damp;

						// If dontUseAnyParticleEffects bool is not selected in RCC_Settings, set emission to ground physic material smoke.
						if (!RCCSettings.dontUseAnyParticleEffects)
							emission = allWheelParticles[physicsMaterials.terrainSplatMapIndex[k]].emission;

						// Set audioclip to ground physic material sound.
						audioClip = physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].groundSound;

						// If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
						if (!RCCSettings.dontUseAnyParticleEffects) {
							if (wheelSlipAmountSideways > physicsFrictions [physicsMaterials.terrainSplatMapIndex [k]].slip || wheelSlipAmountForward > physicsFrictions [physicsMaterials.terrainSplatMapIndex [k]].slip) {
								emission.enabled = true;
							} else {
								emission.enabled = false;
							}
						}
							 
					}

				}
				
			}

		}

		// If wheel still not contacted any of ground material in Configurable Ground Materials, set it to original default values.
		if(!contacted){

			// Setting default stiffness to ground physic material stiffness.
			forwardFrictionCurve.stiffness = orgForwardStiffness;
			sidewaysFrictionCurve.stiffness = orgSidewaysStiffness * tractionHelpedSidewaysStiffness;

			// If drift mode is selected, apply specific frictions.
			if(RCCSettings.behaviorType == RCC_Settings.BehaviorType.Drift){
				Drift();
			}

			// Setting default friction curves to wheels.
			wheelCollider.forwardFriction = forwardFrictionCurve;
			wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

			// Also default damp too.
			wheelCollider.wheelDampingRate = physicsFrictions[0].damp;

			// If dontUseAnyParticleEffects bool is not selected in RCC_Settings, set emission to ground physic material smoke to default one.
			if (!RCCSettings.dontUseAnyParticleEffects)
				emission = allWheelParticles[0].emission;

			// Set audioclip to ground physic material sound to default one.
			audioClip = physicsFrictions[0].groundSound;

			// If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
			if (!RCCSettings.dontUseAnyParticleEffects) {
				
				if (wheelSlipAmountSideways > physicsFrictions [0].slip || wheelSlipAmountForward > physicsFrictions [0].slip) {
					emission.enabled = true;
				} else {
					emission.enabled = false;
				}

			}

		}

		// Last check if wheel has enabled smoke particles. If slip is smaller than target slip value, disable all of them.
		if (!RCCSettings.dontUseAnyParticleEffects) {

			for (int i = 0; i < allWheelParticles.Count; i++) {

				if (wheelSlipAmountSideways > startSlipValue || wheelSlipAmountForward > startSlipValue) {
				
				} else {
					emission = allWheelParticles [i].emission;
					emission.enabled = false;
				}
			
			}

		}

	}

	//
	void Drift(){
		
		Vector3 relativeVelocity = transform.InverseTransformDirection(rigid.velocity);
		float sqrVel = ((relativeVelocity.x * relativeVelocity.x)) / 100f;

		// Forward
		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .1f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .1f, minForwardStiffness);
		}else{
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .75f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .75f,  minForwardStiffness);
		}

		// Sideways
		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel / 1f, .5f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minSidewaysStiffness);
		}else{
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .45f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .45f, minSidewaysStiffness);
		}

	}

	void Audio(){

		if(RCCSettings.useSharedAudioSources && isSkidding())
			return;

		if(totalSlip > startSlipValue){

			if(audioSource.clip != audioClip)
				audioSource.clip = audioClip;

			if(!audioSource.isPlaying)
				audioSource.Play();

			if(rigid.velocity.magnitude > 1f){
				audioSource.volume = Mathf.Lerp(audioSource.volume, Mathf.Lerp(0f, 1f, totalSlip - startSlipValue), Time.deltaTime * 5f);
				audioSource.pitch = Mathf.Lerp(1f, .8f, audioSource.volume);
			}else{
				audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 5f);
			}
			
		}else{
			
			audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 5f);

			if(audioSource.volume <= .05f && audioSource.isPlaying)
				audioSource.Stop();
			
		}

	}

	bool isSkidding(){

		for (int i = 0; i < allWheelColliders.Count; i++) {

			if(allWheelColliders[i].totalSlip > totalSlip)
				return true;

		}

		return false;

	}

	void ApplyMotorTorque(float torque){

		if(carController.TCS){

			WheelHit hit;
			wheelCollider.GetGroundHit(out hit);

			if(Mathf.Abs(wheelCollider.rpm) >= 100){
				if(hit.forwardSlip > .25f){
					carController.TCSAct = true;
					torque -= Mathf.Clamp(torque * (hit.forwardSlip) * carController.TCSStrength, 0f, carController.engineTorque);
				}else{
					carController.TCSAct = false;
					torque += Mathf.Clamp(torque * (hit.forwardSlip) * carController.TCSStrength, -carController.engineTorque, 0f);
				}
			}else{
				carController.TCSAct = false;
			}

		}

		if(OverTorque())
			torque = 0;

		wheelCollider.motorTorque = ((torque * (1 - carController.clutchInput) * carController._boostInput) * carController._gasInput) * (carController.engineTorqueCurve[carController.currentGear].Evaluate(wheelRPMToSpeed * carController.direction) * carController.direction);

		carController.ApplyEngineSound(wheelCollider.motorTorque);

	}

	public void ApplySteering(){

		if(carController.applyCounterSteering && carController.currentGear != 0)
			wheelCollider.steerAngle = Mathf.Clamp((carController.steerAngle * (carController._steerInput + carController.driftAngle)), -carController.steerAngle, carController.steerAngle);
		else
			wheelCollider.steerAngle = Mathf.Clamp((carController.steerAngle * carController._steerInput), -carController.steerAngle, carController.steerAngle);

	}

	void ApplyBrakeTorque(float brake){

		if(carController.ABS && carController.handbrakeInput <= .1f){

			WheelHit hit;
			wheelCollider.GetGroundHit(out hit);

			if((Mathf.Abs(hit.forwardSlip) * Mathf.Clamp01(brake)) >= carController.ABSThreshold){
				carController.ABSAct = true;
				brake = 0;
			}else{
				carController.ABSAct = false;
			}

		}

		wheelCollider.brakeTorque = brake;

	}

	bool OverTorque(){

		if(carController.speed > carController.maxspeed || !carController.engineRunning)
			return true;

		return false;

	}

}