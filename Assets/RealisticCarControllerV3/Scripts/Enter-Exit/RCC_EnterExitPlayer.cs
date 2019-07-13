//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCC_EnterExitPlayer : MonoBehaviour {

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

	public PlayerType playerType;
	public enum PlayerType{FPS, TPS}

	public GameObject rootOfPlayer;
	public float maxRayDistance= 2f;
	public float rayHeight = 1f;
	public GameObject TPSCamera;
	private bool showGui = false;

	void Start(){

		if (!rootOfPlayer)
			rootOfPlayer = transform.root.gameObject;

		GameObject carCamera = GameObject.FindObjectOfType<RCC_Camera>().gameObject;
		carCamera.SetActive(false);

	}
	
	void Update (){
		
		Vector3 direction= transform.TransformDirection(Vector3.forward);
		RaycastHit hit;

		if(Physics.Raycast(new Vector3(transform.position.x, transform.position.y + (playerType == PlayerType.TPS ? rayHeight : 0f), transform.position.z), direction, out hit, maxRayDistance)){

			if(hit.transform.GetComponentInParent<RCC_EnterExitCar>()){

				showGui = true;

				if (Input.GetKeyDown (RCCSettings.enterExitVehicleKB)) {

					hit.transform.GetComponentInParent<RCC_CarControllerV3> ().SendMessage ("Act", rootOfPlayer, SendMessageOptions.DontRequireReceiver);
					
				}

			}else{

				showGui = false;

			}
			
		}else{

			showGui = false;

		}
		
	}
	
	void OnGUI (){
		
		if(showGui){
			if(RCCSettings.controllerType == RCC_Settings.ControllerType.Keyboard)
				GUI.Label( new Rect(Screen.width - (Screen.width/1.7f),Screen.height - (Screen.height/1.2f),800,100),"Press ''" + RCCSettings.enterExitVehicleKB.ToString() + "'' key to Get In");
		}
		
	}

	void OnDrawGizmos(){

		Gizmos.color = Color.red;
		Gizmos.DrawRay (new Vector3(transform.position.x, transform.position.y + (playerType == PlayerType.TPS ? rayHeight : 0f), transform.position.z), transform.forward * maxRayDistance);
		
	}

	void OnEnable(){

		if (TPSCamera)
			TPSCamera.SetActive (true);

	}

	void OnDisable(){

		if (TPSCamera)
			TPSCamera.SetActive (false);

	}
	
}