//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Main RCC Camera controller. Includes 7 different camera modes with many customizable settings. It doesn't use different cameras on your scene like *other* assets. Simply it parents the camera to their positions that's all. No need to be Einstein.
/// Also supports collision detection for this new version (V3.1).
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Camera/Main Camera")]
public class RCC_Camera : MonoBehaviour{

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
	
	// The target we are following transform and rigidbody.
	public Transform playerCar;
	public Transform _playerCar{get{return playerCar;}set{playerCar = value;	GetPlayerCar();}}
	private Rigidbody playerRigid;

	private Camera cam;	// Camera is not attached to this main gameobject. Our child camera parented to this gameobject. Therefore, we can apply additional position and rotation changes.
	public GameObject pivot;	// Pivot center of the camera. Used for making offsets and collision movements.

	// Camera modes.
	public CameraMode cameraMode;
	public enum CameraMode{TPS, FPS, WHEEL, FIXED, CINEMATIC, TOP, ORBIT}

	public bool useTopCameraMode = false;	// Shall we use top camera mode?
	public bool useHoodCameraMode = true;	// Shall we use hood camera mode?
	public bool useWheelCameraMode = true;	// Shall we use wheel camera mode?
	public bool useFixedCameraMode = true;	// Shall we use fixed camera mode?
	public bool useOrbitCameraMode = false;	// Shall we use top camera mode?
	public bool useCinematicCameraMode = false;	// Shall we use top camera mode?

	public bool useOrthoForTopCamera = true;		// Use Ortho or Perspective camera mode.
	public Vector3 topCameraAngle = new Vector3(45f, 45f, 0f);	// If so, we will use this Vector3 angle for top camera mode.

	private float distanceOffset = 0f;	
	public float maximumZDistanceOffset = 10f;	// Distance offset for top camera mode. Related with car speed. If car speed is higher, camera will be shifted to front of the car.
	public float topCameraDistance = 100f;	// Top camera height / distance.

	// They are used for smooth camera movements for top camera mode. Smooth camera movements are only used for Top camera mode.
	private Vector3 targetPosition, pastFollowerPosition = Vector3.zero;
	private Vector3 pastTargetPosition = Vector3.zero;

	// The distance for TPS camera mode.
	public float TPSDistance = 6f;
	
	// The height we want the camera to be above the target for TPS camera mode.
	public float TPSHeight = 1.5f;

	public float TPSHeightDamping = 5f;
	public float TPSRotationDamping = 3f;

	internal float targetFieldOfView = 60f;	// Camera will adapt its field of view to this target field of view. All field of views above this line will feed this float value.

	public float TPSMinimumFOV = 50f;		// Minimum field of view related with car speed.
	public float TPSMaximumFOV = 60f;	// Maximum field of view related with car speed.
	public float hoodCameraFOV = 65f;		// Hood field of view.
	public float wheelCameraFOV = 60f;	// Wheel field of view.
	public float minimumOrtSize = 10f;		// Minimum ortho size related with car speed.
	public float maximumOrtSize = 20f;		// Maximum ortho size related with car speed.
	public float orbitCameraFOV = 50f;		// Orbit camera field of view.
	
	public float maximumTilt = 15f;	// Maximum tilt angle related with rigidbody angular velocity X.
	private float tiltAngle = 0f;	// Current tilt angle.

	internal int cameraSwitchCount = 0;		// Used in switch case for running corresponding camera mode method.
	private RCC_HoodCamera hoodCam;		// Hood camera. It's a null script. Just used for finding hood camera parented to our player car.
	private RCC_WheelCamera wheelCam;		// Wheel camera. It's a null script. Just used for finding wheel camera parented to our player car.
	private RCC_FixedCamera fixedCam;		// Fixed Camera System.
	private RCC_CinematicCamera cinematicCam;		// Fixed Camera System.

	private float speed = 0f;		// Vehicle speed.

	private Vector3 collisionVector;	// Collision vector.
	private Vector3 collisionPos;		// Collision position.
	private Quaternion collisionRot;	// Collision rotation.

	private float index = 0f;		// Used for sinus FOV effect after hard crashes. 

	// Orbit X and Y inputs.
	private float orbitX = 0f;
	private float orbitY = 0f;

	//	Smoothed Orbit X and Y inputs if you wanna use ''Smooth Orbit''.
	private float smoothedOrbitX;
	private float smoothedOrbitY;

	public bool useSmoothOrbit = false;	// Smooth Orbit.
	public bool useOnlyWhenHold = false;	// Only use Orbit while clicking / touching.

	// Minimum and maximum Orbit X, Y degrees.
	public float minOrbitY = -20f;
	public float maxOrbitY = 80f;

	//	Orbit X and Y speeds.
	public float orbitXSpeed = 10f;
	public float orbitYSpeed = 10f;

	//	Orbit transform.
	private Vector3 orbitPosition;
	private Quaternion orbitRotation;

	private float orgTimeScale = 1f;

	void Awake(){

		cam = GetComponentInChildren<Camera>();
		orgTimeScale = Time.timeScale;

	}
	
	void GetPlayerCar(){

		// Return if we don't have player car.
		if(!playerCar)
			return;

		// If player car has RCC_CameraConfig, distance and height will be changed.
		if (playerCar.GetComponent<RCC_CameraConfig> ()) {
			TPSDistance = playerCar.GetComponent<RCC_CameraConfig> ().distance;
			TPSHeight = playerCar.GetComponent<RCC_CameraConfig> ().height;
		}

		ChangeCamera (CameraMode.TPS);

		playerRigid = playerCar.GetComponent<Rigidbody>();

		// Getting camera modes from player car.
		hoodCam = playerCar.GetComponentInChildren<RCC_HoodCamera>();
		wheelCam = playerCar.GetComponentInChildren<RCC_WheelCamera>();
		fixedCam = GameObject.FindObjectOfType<RCC_FixedCamera>();
		cinematicCam = GameObject.FindObjectOfType<RCC_CinematicCamera>();

		// Setting transform and position to players car when switched camera target.
		transform.position = playerCar.transform.position;
		transform.rotation = playerCar.transform.rotation * Quaternion.Euler(10f, 0f, 0f);

	}

	public void SetPlayerCar(GameObject player){

		playerCar = player.transform;
		GetPlayerCar ();

	}
	
	void Update(){
		
		// Early out if we don't have a player.
		if (!playerCar || !playerRigid){
			GetPlayerCar();
			return;
		}

		// Speed of the vehicle (smoothed).
		speed = Mathf.Lerp(speed, playerCar.InverseTransformDirection(playerRigid.velocity).z * 3.6f, Time.deltaTime * 3f);

		// Used for sinus FOV effect after hard crashes. 
		if(index > 0)
			index -= Time.deltaTime * 5f;

		// Lerping current field of view to target field of view.
		cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, targetFieldOfView, Time.deltaTime * 5f);

		if (Input.GetKey (RCCSettings.slowMotionKB))
			Time.timeScale = .2f;
		
		if (Input.GetKeyUp (RCCSettings.slowMotionKB))
			Time.timeScale = orgTimeScale;
			
	}
	
	void LateUpdate (){
		
		// Early out if we don't have a target.
		if (!playerCar || !playerRigid)
			return;

		// Even if we have the player and it's disabled, return.
		if (!playerCar.gameObject.activeSelf)
			return;

		// Run the corresponding method with choosen camera mode.
		switch(cameraMode){

		case CameraMode.TPS:
			TPS();
			break;

		case CameraMode.FPS:
			FPS();
			break;
		
		case CameraMode.WHEEL:
			WHEEL();
			break;

		case CameraMode.FIXED:
			FIXED();
			break;

		case CameraMode.CINEMATIC:
			CINEMATIC();
			break;

		case CameraMode.TOP:
			TOP();
			break;

		case CameraMode.ORBIT:
			ORBIT();
			break;

		}

		if(Input.GetKeyDown(RCCSettings.changeCameraKB)){
			ChangeCamera();
		}

	}

	// Change camera by increasing camera switch counter.
	public void ChangeCamera(){

		// Increasing camera switch counter at each camera changing.
		cameraSwitchCount ++;

		// We have 7 camera modes at total. If camera switch counter is greater than maximum, set it to 0.
		if (cameraSwitchCount >= 7) {
			cameraSwitchCount = 0;
			cameraMode = 0;
		}

		switch(cameraSwitchCount){

		case 0:
			cameraMode = CameraMode.TPS;
			break;

		case 1:
			if(useHoodCameraMode && hoodCam){
				cameraMode = CameraMode.FPS;
			}else{
				ChangeCamera();
			}
			break;

		case 2:
			if(useWheelCameraMode && wheelCam){
				cameraMode = CameraMode.WHEEL;
			}else{
				ChangeCamera();
			}
			break;

		case 3:
			if(useFixedCameraMode && fixedCam){
				cameraMode = CameraMode.FIXED;
			}else{
				ChangeCamera();
			}
			break;

		case 4:
			if(useCinematicCameraMode){
				cameraMode = CameraMode.CINEMATIC;
			}else{
				ChangeCamera();
			}
			break;

		case 5:
			if(useTopCameraMode){
				cameraMode = CameraMode.TOP;
			}else{
				ChangeCamera();
			}
			break;

		case 6:
			if(useOrbitCameraMode){
				cameraMode = CameraMode.ORBIT;
			}else{
				ChangeCamera();
			}
			break;


		}

		// Resetting camera when changing camera mode.
		ResetCamera ();

	}

	// Change camera by directly setting it to specific mode.
	public void ChangeCamera(CameraMode mode){

		cameraMode = mode;

		// Resetting camera when changing camera mode.
		ResetCamera ();

	}

	void FPS(){

	}

	void WHEEL(){

	}

	void TPS(){

		// Lerping targetFieldOfView from TPSMinimumFOV to TPSMaximumFOV related with car speed.
		targetFieldOfView = Mathf.Lerp(TPSMinimumFOV, TPSMaximumFOV, Mathf.Abs(speed) / 150f);

		// Sinus FOV effect on hard crashes.
		targetFieldOfView += (5f * Mathf.Cos (1f * index));

		// Rotates camera by Z axis for tilt effect.
		tiltAngle = Mathf.Lerp(0f, maximumTilt * (int)Mathf.Clamp(-playerCar.InverseTransformDirection(playerRigid.velocity).x, -1f, 1f), Mathf.Abs(playerCar.InverseTransformDirection(playerRigid.velocity).x) / 50f);

		// Calculate the current rotation angles for TPS mode.
		float wantedRotationAngle = playerCar.eulerAngles.y;
		float wantedHeight = playerCar.position.y + TPSHeight;
		float currentRotationAngle = transform.eulerAngles.y;
		float currentHeight = transform.position.y;

		TPSRotationDamping = Mathf.Lerp(0f, 5f, Mathf.Abs(speed) / 40f);

		// If speed is lower than -10, rotate the camera to backwards.
		if(speed < -10)
			wantedRotationAngle = playerCar.eulerAngles.y + 180;

		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, TPSRotationDamping * Time.deltaTime);

		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, TPSHeightDamping * Time.deltaTime);

		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);

		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		transform.position = playerCar.position;
		transform.position -= currentRotation * Vector3.forward * TPSDistance;

		// Set the height of the camera
		transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

		// Always look at the target
		transform.LookAt (new Vector3(playerCar.position.x, playerCar.position.y + 1f, playerCar.position.z));
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y, Mathf.Clamp(tiltAngle, -10f, 10f));

		// Collision positions and rotations that affects pivot of the camera.
		collisionPos = Vector3.Lerp(collisionPos, Vector3.zero, Time.deltaTime * 5f);
		if(Time.deltaTime != 0)
		collisionRot = Quaternion.Lerp(collisionRot, Quaternion.identity, Time.deltaTime * 5f);
		pivot.transform.localPosition = Vector3.Lerp(pivot.transform.localPosition, collisionPos, Time.deltaTime * 5f);
		pivot.transform.localRotation = Quaternion.Lerp(pivot.transform.localRotation, collisionRot, Time.deltaTime * 5f);

	}

	void FIXED(){

		if(fixedCam.transform.parent != null)
			fixedCam.transform.SetParent(null);

	}

	void TOP(){

		// Early out if we don't have a target.
		if (!playerCar || !playerRigid)
			return;

		// Setting proper targetPosition for Top camera mode.
		targetPosition = playerCar.position;
		targetPosition += playerCar.rotation * Vector3.forward * distanceOffset;

		// Setting ortho or perspective?
		cam.orthographic = useOrthoForTopCamera;

		distanceOffset = Mathf.Lerp (0f, maximumZDistanceOffset, Mathf.Abs(speed) / 100f);
		targetFieldOfView = Mathf.Lerp (minimumOrtSize, maximumOrtSize, Mathf.Abs(speed) / 100f);
		cam.orthographicSize = targetFieldOfView;

		transform.position = SmoothApproach(pastFollowerPosition, pastTargetPosition, targetPosition, Mathf.Clamp(.5f, Mathf.Abs(speed), Mathf.Infinity));
		transform.rotation = Quaternion.Euler (topCameraAngle);

		pastFollowerPosition = transform.position;
		pastTargetPosition = targetPosition;

		pivot.transform.localPosition = new Vector3 (0f, 0f, -topCameraDistance);

	}

	void ORBIT(){

		if (!useOnlyWhenHold) {
			
			if (useSmoothOrbit) {

				smoothedOrbitX += Input.GetAxis ("Mouse X") * (orbitXSpeed * 10f) * Time.deltaTime;
				smoothedOrbitY -= Input.GetAxis ("Mouse Y") * (orbitYSpeed * 10f) * Time.deltaTime;
				smoothedOrbitY = Mathf.Clamp (smoothedOrbitY, minOrbitY, maxOrbitY);

				orbitX = Mathf.Lerp (orbitX, smoothedOrbitX, Time.deltaTime * 10f);
				orbitY = Mathf.Lerp (orbitY, smoothedOrbitY, Time.deltaTime * 10f);

			} else {
				
				orbitX += Input.GetAxis ("Mouse X") * (orbitXSpeed * 10f) * Time.deltaTime;
				orbitY -= Input.GetAxis ("Mouse Y") * (orbitYSpeed * 10f) * Time.deltaTime;

			}

		}

		orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

		orbitRotation = Quaternion.Euler(orbitY, orbitX, 0);
		orbitPosition = orbitRotation * new Vector3(0f, 0f, -TPSDistance) + playerCar.position;

		transform.rotation = orbitRotation;
		transform.position = orbitPosition;

	}

	void CINEMATIC(){

		if(cinematicCam.transform.parent != null)
			cinematicCam.transform.SetParent(null);

		targetFieldOfView = cinematicCam.targetFOV;

	}

	public void OnDrag(BaseEventData data){

		PointerEventData pointerData = data as PointerEventData;

		orbitX += pointerData.delta.x * orbitXSpeed * .02f;
		orbitY -= pointerData.delta.y * orbitYSpeed * .02f;

		orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

		orbitRotation = Quaternion.Euler(orbitY, orbitX, 0);
		orbitPosition = orbitRotation * new Vector3(0f, 0f, -TPSDistance) + playerCar.position;

	}

	public void Collision(Collision collision){

		if(!enabled || cameraMode != CameraMode.TPS)
			return;
		
		Vector3 colRelVel = collision.relativeVelocity;
		colRelVel *= 1f - Mathf.Abs(Vector3.Dot(transform.up,collision.contacts[0].normal));

		float cos = Mathf.Abs(Vector3.Dot(collision.contacts[0].normal, colRelVel.normalized));

		if (colRelVel.magnitude * cos >= 5f){

			collisionVector = transform.InverseTransformDirection(colRelVel) / (30f);

			collisionPos -= collisionVector * 5f;
			collisionRot = Quaternion.Euler(new Vector3(-collisionVector.z * 50f, -collisionVector.y * 50f, -collisionVector.x * 50f));
			targetFieldOfView = cam.fieldOfView - Mathf.Clamp(collision.relativeVelocity.magnitude, 0f, 15f);
			index = Mathf.Clamp(collision.relativeVelocity.magnitude / 5f, 0f, 10f);

		}

	}

	private void ResetCamera(){

		if(fixedCam)
			fixedCam.canTrackNow = false;

		tiltAngle = 0f;

		collisionPos = Vector3.zero;
		collisionRot = Quaternion.identity;

		pivot.transform.localPosition = collisionPos;
		pivot.transform.localRotation = collisionRot;

		cam.orthographic = false;

		switch (cameraMode) {

		case CameraMode.TPS:
			transform.SetParent(null);
			targetFieldOfView = TPSMaximumFOV;
			break;

		case CameraMode.FPS:
			transform.SetParent (hoodCam.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = hoodCameraFOV;
			hoodCam.FixShake ();
			break;

		case CameraMode.WHEEL:
			transform.SetParent(wheelCam.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = wheelCameraFOV;
			break;

		case CameraMode.FIXED:
			transform.SetParent(fixedCam.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = 60;
			fixedCam.currentCar = playerCar;
			fixedCam.canTrackNow = true;
			break;

		case CameraMode.CINEMATIC:
			transform.SetParent(cinematicCam.pivot.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = 30f;
			cinematicCam.currentCar = playerCar;
			break;

		case CameraMode.TOP:
			transform.SetParent(null);
			targetFieldOfView = minimumOrtSize;
			pivot.transform.localPosition = Vector3.zero;
			pivot.transform.localRotation = Quaternion.identity;
			targetPosition = playerCar.position;
			targetPosition += playerCar.rotation * Vector3.forward * distanceOffset;
			transform.position = playerCar.position;
			pastFollowerPosition = playerCar.position;
			pastTargetPosition = targetPosition;
			break;

		case CameraMode.ORBIT:
			transform.SetParent(null);
			targetFieldOfView = orbitCameraFOV;
			orbitX = 0f;
			orbitY = 0f;
			smoothedOrbitX = 0f;
			smoothedOrbitY = 0f;
			break;

		}

	}

	// Used for smooth position lerping.
	private Vector3 SmoothApproach( Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float delta){

		if(float.IsNaN(delta) || float.IsInfinity(delta) || pastPosition == Vector3.zero || pastTargetPosition == Vector3.zero || targetPosition == Vector3.zero)
			return transform.position;

		float t = Time.deltaTime * delta;
		Vector3 v = ( targetPosition - pastTargetPosition ) / t;
		Vector3 f = pastPosition - pastTargetPosition + v;
		return targetPosition - v + f * Mathf.Exp( -t );

	}

}