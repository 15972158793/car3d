//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// A simple manager script for all demo scenes. It has an array of spawnable player cars and public methods for spawning new cars, setting new behavior modes, restart, and quit application.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/Demo Manager")]
public class RCC_Demo : MonoBehaviour {

	[Header("Spawnable Cars")]
	public RCC_CarControllerV3[] selectableVehicles;

	private int selectedCarIndex = 0;		// An integer index value used for spawning a new car.
	private int selectedBehaviorIndex = 0;		// An integer index value used for setting behavior mode.

	// An integer index value used for spawning a new car.
	public void SelectVehicle (int index) {

		selectedCarIndex = index;
	
	}

	public void Spawn () {

		// Getting all RCC cars on scene.
		RCC_CarControllerV3[] activeVehicles = GameObject.FindObjectsOfType<RCC_CarControllerV3>();

		// Last known position and rotation of last active car.
		Vector3 lastKnownPos = new Vector3();
		Quaternion lastKnownRot = new Quaternion();

		// Checking if there is at least one car on scene.
		if(activeVehicles != null && activeVehicles.Length > 0){

			// Checking if car is AI or not. If it's not AI and controllable by player, last known position and rotation will be assigned to this car.
			// We will use this position and rotation for new spawned car.
			foreach(RCC_CarControllerV3 rcc in activeVehicles){
				if(!rcc.AIController && rcc.canControl){
					lastKnownPos = rcc.transform.position;
					lastKnownRot = rcc.transform.rotation;
					break;
				}
			}

		}

		// If last known position and rotation is not assigned, camera's position and rotation will be used.
		if(lastKnownPos == Vector3.zero){
			if(	GameObject.FindObjectOfType<RCC_Camera>()){
				lastKnownPos = GameObject.FindObjectOfType<RCC_Camera>().transform.position;
				lastKnownRot = GameObject.FindObjectOfType<RCC_Camera>().transform.rotation;
			}
		}

		// We don't need X and Z rotation angle. Just Y.
		lastKnownRot.x = 0f;
		lastKnownRot.z = 0f;

		for (int i = 0; i < activeVehicles.Length; i++) {

			// If we have controllable cars by players on scene, destroy them!
			if(activeVehicles[i].canControl && !activeVehicles[i].AIController){
				Destroy(activeVehicles[i].gameObject);
			}
			 
		}

		// Here we are creating a new gameobject for our new spawner car.
		GameObject newVehicle = (GameObject)GameObject.Instantiate(selectableVehicles[selectedCarIndex].gameObject, lastKnownPos + (Vector3.up), lastKnownRot);
		 
		// Enabling canControl bool for our new car.
		newVehicle.GetComponent<RCC_CarControllerV3>().canControl = true;

		// Setting new target of RCC Camera to our new car.
		if(	GameObject.FindObjectOfType<RCC_Camera>()){
			GameObject.FindObjectOfType<RCC_Camera>().SetPlayerCar(newVehicle);
		}

		// If our scene has RCC Customizer Example, this will set target car of that customizer and checks all UI elements belongs to customization.
		if (GameObject.FindObjectOfType<RCC_CustomizerExample> ()) {
			GameObject.FindObjectOfType<RCC_CustomizerExample> ().car = newVehicle.GetComponent<RCC_CarControllerV3> ();
			GameObject.FindObjectOfType<RCC_CustomizerExample> ().CheckUIs ();
		}

	}

	// An integer index value used for setting behavior mode.
	public void SelectBehavior(int index){

		selectedBehaviorIndex = index;

	}

	// Here we are setting new selected behavior to corresponding one.
	public void InitBehavior(){

		switch(selectedBehaviorIndex){
		case 0:
			RCC_Settings.Instance.behaviorType = RCC_Settings.BehaviorType.Simulator;
			RestartScene();
			break;
		case 1:
			RCC_Settings.Instance.behaviorType = RCC_Settings.BehaviorType.Racing;
			RestartScene();
			break;
		case 2:
			RCC_Settings.Instance.behaviorType = RCC_Settings.BehaviorType.SemiArcade;
			RestartScene();
			break;
		case 3:
			RCC_Settings.Instance.behaviorType = RCC_Settings.BehaviorType.Drift;
			RestartScene();
			break;
		case 4:
			RCC_Settings.Instance.behaviorType = RCC_Settings.BehaviorType.Fun;
			RestartScene();
			break;
		case 5:
			RCC_Settings.Instance.behaviorType = RCC_Settings.BehaviorType.Custom;
			RestartScene();
			break;
		}

	}

	// Simply restarting the current scene.
	public void RestartScene(){

		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);

	}

	// Simply quit application. Not working on Editor.
	public void Quit(){

		Application.Quit();

	}

}
