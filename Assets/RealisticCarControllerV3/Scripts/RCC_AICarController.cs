//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2016 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI Controller of RCC. It's not professional, but it does the job. Follows all waypoints, or chases the player.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/AI/AI Controller")]
public class RCC_AICarController : MonoBehaviour {

	private RCC_CarControllerV3 carController;		// Main RCC Car Controller of this vehicle.
	private Rigidbody rigid;		// Rigidbody of this vehicle.
	
	// Waypoint Container.
	public RCC_AIWaypointsContainer waypointsContainer;
	public int currentWaypoint = 0;

	public Transform targetChase;		// Target Gameobject for chasing.

	// AI Type
	public AIType _AIType;
	public enum AIType {FollowWaypoints, ChasePlayer}
	
	// Raycast distances used for detecting obstacles front of our AI car.
	public LayerMask obstacleLayers = -1;
	public int wideRayLength = 20;
	public int tightRayLength = 20;
	public int sideRayLength = 3;

	private float rayInput = 0f;		// Total ray input affected by raycast distances.

	private bool  raycasting = false;		// Raycasts hits an obstacle now?

	private float resetTime = 0f;		// This float timer value was used for deciding go back or not after crashing.
	
	// Steer, motor, and brake inputs. This script will feed RCC_CarController with these inputs.
	private float steerInput = 0f;
	private float gasInput = 0f;
	private float brakeInput = 0f;

	// Limit Speed.
	public bool limitSpeed = false;
	public float maximumSpeed = 100f;

	// Smoothed steering.
	public bool smoothedSteer = true;
	
	// Brake Zone.
	private float maximumSpeedInBrakeZone = 0f;
	private bool inBrakeZone = false;
	
	// Counts laps and how many waypoints passed.
	public int lap = 0;
	public int totalWaypointPassed = 0;
	public int nextWaypointPassRadius = 40;
	public bool ignoreWaypointNow = false;
	
	// Unity's Navigator.
	private UnityEngine.AI.NavMeshAgent navigator;
	private GameObject navigatorObject;

	void Awake() {

		carController = GetComponent<RCC_CarControllerV3>();
		carController.AIController = true;
		rigid = GetComponent<Rigidbody>();

		// If Waypoints Container is not selected in Inspector Panel, find it on scene.
		if(!waypointsContainer)
			waypointsContainer = FindObjectOfType(typeof(RCC_AIWaypointsContainer)) as RCC_AIWaypointsContainer;

		// Creating our Navigator and setting properties.
		navigatorObject = new GameObject("Navigator");
		navigatorObject.transform.parent = transform;
		navigatorObject.transform.localPosition = Vector3.zero;
		navigatorObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
		navigator = navigatorObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
		navigator.radius = 1;
		navigator.speed = 1;
		navigator.angularSpeed = 1000f;
		navigator.height = 1;
		navigator.avoidancePriority = 50;

	}
	
	void Update(){

		// Assigning navigator's position to front wheels of the car.
		navigator.transform.localPosition = new Vector3(0, carController.FrontLeftWheelCollider.transform.localPosition.y, carController.FrontLeftWheelCollider.transform.localPosition.z);

	}
	
	void  FixedUpdate (){

		// If not controllable, no need to go further.
		if(!carController.canControl)
			return;

		Navigation();		// Feeds steerInput based on navigator.
		FixedRaycasts();		// Affects steerInput if one of raycasts detects an object front of our AI car.
		FeedRCC();		// Feeds motorInput.
		Resetting();		// Was used for deciding go back or not after crashing.

	}
	
	void Navigation (){

		// If our scene doesn't have a Waypoint Container, return with error.
		if(_AIType == AIType.FollowWaypoints && !waypointsContainer){
			Debug.LogError("Waypoints Container Couldn't Found!");
			enabled = false;
			return;
		}

		// If our scene has Waypoint Container and it doesn't have any waypoints, return with error.
		if(_AIType == AIType.FollowWaypoints && waypointsContainer && waypointsContainer.waypoints.Count < 1){
			Debug.LogError("Waypoints Container Doesn't Have Any Waypoints!");
			enabled = false;
			return;
		}

		// If our scene doesn't have a Waypoint Container, return with error.
		if(_AIType == AIType.ChasePlayer && !targetChase){
			Debug.LogError("Target Chase Couldn't Found!");
			enabled = false;
			return;
		}
		
		// Next waypoint's position.
		Vector3 nextWaypointPosition = transform.InverseTransformPoint( new Vector3(waypointsContainer.waypoints[currentWaypoint].position.x, transform.position.y, waypointsContainer.waypoints[currentWaypoint].position.z));

		// Navigator Input is multiplied by 1.5f for fast reactions.
		float navigatorInput = Mathf.Clamp(transform.InverseTransformDirection(navigator.desiredVelocity).x * 1.5f, -1f, 1f);

		// Setting destination of the Navigator. 
		if (_AIType == AIType.FollowWaypoints) {
			if(navigator.isOnNavMesh)
				navigator.SetDestination (waypointsContainer.waypoints [currentWaypoint].position);
		} else {
			if(navigator.isOnNavMesh)
				navigator.SetDestination (targetChase.position);
		}

		// Steer Input.
		if(carController.direction == 1){
			if(!ignoreWaypointNow)
				steerInput = Mathf.Clamp((navigatorInput + rayInput), -1f, 1f);
			else
				steerInput = Mathf.Clamp(rayInput, -1f, 1f);
		}else{
			steerInput = Mathf.Clamp((-navigatorInput - rayInput), -1f, 1f);
		}

		// Brake Input.
		if(!inBrakeZone){
			if(carController.speed >= 25){
				brakeInput = Mathf.Lerp(0f, .85f, (Mathf.Abs(steerInput)));
			}else{
				brakeInput = 0f;
			}
		}else{
			brakeInput = Mathf.Lerp(0f, 1f, (carController.speed - maximumSpeedInBrakeZone) / maximumSpeedInBrakeZone);
		}

		// Gas Input.
		if(!inBrakeZone){
			
			if(carController.speed >= 10){
				if(!carController.changingGear)
					gasInput = Mathf.Clamp(1f - (Mathf.Abs(navigatorInput / 10f)  - Mathf.Abs(rayInput / 10f)), .75f, 1f);
				else
					gasInput = 0f;
			}else{
				if(!carController.changingGear)
					gasInput = 1f;
				else
					gasInput = 0f;
			}

		}else{
			
			if(!carController.changingGear)
				gasInput = Mathf.Lerp(1f, 0f, (carController.speed) / maximumSpeedInBrakeZone);
			else
				gasInput = 0f;

		}

		if (_AIType == AIType.FollowWaypoints) {
		
			// Checks for the distance to next waypoint. If it is less than written value, then pass to next waypoint.
			if (nextWaypointPosition.magnitude < nextWaypointPassRadius) {
				
				currentWaypoint++;
				totalWaypointPassed++;
			
				// If all waypoints are passed, sets the current waypoint to first waypoint and increase lap.
				if (currentWaypoint >= waypointsContainer.waypoints.Count) {
					currentWaypoint = 0;
					lap++;
				}

			}

		}
		
	}
		
	void Resetting (){

		// If unable to move forward, puts the gear to R.

		if(carController.speed <= 5 && transform.InverseTransformDirection(rigid.velocity).z < 1f)
			resetTime += Time.deltaTime;
		
		if(resetTime >= 2)
			carController.direction = -1;

		if(resetTime >= 4 || carController.speed >= 25){
			carController.direction = 1;
			resetTime = 0;
		}
		
	}
	
	void FixedRaycasts(){
		
		Vector3 forward = transform.TransformDirection ( new Vector3(0, 0, 1));
		Vector3 pivotPos = new Vector3(transform.localPosition.x, carController.FrontLeftWheelCollider.transform.position.y, transform.localPosition.z);
		RaycastHit hit;
		
		// New bools effected by fixed raycasts.
		bool  tightTurn = false;
		bool  wideTurn = false;
		bool  sideTurn = false;
		bool  tightTurn1 = false;
		bool  wideTurn1 = false;
		bool  sideTurn1 = false;
		
		// New input steers effected by fixed raycasts.
		float newinputSteer1 = 0f;
		float newinputSteer2 = 0f;
		float newinputSteer3 = 0f;
		float newinputSteer4 = 0f;
		float newinputSteer5 = 0f;
		float newinputSteer6 = 0f;
		
		// Drawing Rays.
		Debug.DrawRay (pivotPos, Quaternion.AngleAxis(25, transform.up) * forward * wideRayLength, Color.white);
		Debug.DrawRay (pivotPos, Quaternion.AngleAxis(-25, transform.up) * forward * wideRayLength, Color.white);
		
		Debug.DrawRay (pivotPos, Quaternion.AngleAxis(7, transform.up) * forward * tightRayLength, Color.white);
		Debug.DrawRay (pivotPos, Quaternion.AngleAxis(-7, transform.up) * forward * tightRayLength, Color.white);

		Debug.DrawRay (pivotPos, Quaternion.AngleAxis(90, transform.up) * forward * sideRayLength, Color.white);
		Debug.DrawRay (pivotPos, Quaternion.AngleAxis(-90, transform.up) * forward * sideRayLength, Color.white);
		
		// Wide Raycasts.
		if (Physics.Raycast (pivotPos, Quaternion.AngleAxis(25, transform.up) * forward, out hit, wideRayLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {
			Debug.DrawRay (pivotPos, Quaternion.AngleAxis(25, transform.up) * forward * wideRayLength, Color.red);
			newinputSteer1 = Mathf.Lerp (-.5f, 0f, (hit.distance / wideRayLength));
			wideTurn = true;
		}
		
		else{
			newinputSteer1 = 0f;
			wideTurn = false;
		}
		
		if (Physics.Raycast (pivotPos, Quaternion.AngleAxis(-25, transform.up) * forward, out hit, wideRayLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {
			Debug.DrawRay (pivotPos, Quaternion.AngleAxis(-25, transform.up) * forward * wideRayLength, Color.red);
			newinputSteer4 = Mathf.Lerp (.5f, 0f, (hit.distance / wideRayLength));
			wideTurn1 = true;
		}else{
			newinputSteer4 = 0f;
			wideTurn1 = false;
		}
		
		// Tight Raycasts.
		if (Physics.Raycast (pivotPos, Quaternion.AngleAxis(7, transform.up) * forward, out hit, tightRayLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {
			Debug.DrawRay (pivotPos, Quaternion.AngleAxis(7, transform.up) * forward * tightRayLength , Color.red);
			newinputSteer3 = Mathf.Lerp (-1f, 0f, (hit.distance / tightRayLength));
			tightTurn = true;
		}else{
			newinputSteer3 = 0f;
			tightTurn = false;
		}
		
		if (Physics.Raycast (pivotPos, Quaternion.AngleAxis(-7, transform.up) * forward, out hit, tightRayLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {
			Debug.DrawRay (pivotPos, Quaternion.AngleAxis(-7, transform.up) * forward * tightRayLength, Color.red);
			newinputSteer2 = Mathf.Lerp (1f, 0f, (hit.distance / tightRayLength));
			tightTurn1 = true;
		}else{
			newinputSteer2 = 0f;
			tightTurn1 = false;
		}

		// Side Raycasts.
		if (Physics.Raycast (pivotPos, Quaternion.AngleAxis(90, transform.up) * forward, out hit, sideRayLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {
			Debug.DrawRay (pivotPos, Quaternion.AngleAxis(90, transform.up) * forward * sideRayLength , Color.red);
			newinputSteer5 = Mathf.Lerp (-1f, 0f, (hit.distance / sideRayLength));
			sideTurn = true;
		}else{
			newinputSteer5 = 0f;
			sideTurn = false;
		}
		
		if (Physics.Raycast (pivotPos, Quaternion.AngleAxis(-90, transform.up) * forward, out hit, sideRayLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {
			Debug.DrawRay (pivotPos, Quaternion.AngleAxis(-90, transform.up) * forward * sideRayLength, Color.red);
			newinputSteer6 = Mathf.Lerp (1f, 0f, (hit.distance / sideRayLength));
			sideTurn1 = true;
		}else{
			newinputSteer6 = 0f;
			sideTurn1 = false;
		}

		// Raycasts hits an obstacle now?
		if(wideTurn || wideTurn1 || tightTurn || tightTurn1 || sideTurn || sideTurn1)
			raycasting = true;
		else
			raycasting = false;

		// If raycast hits a collider, feed rayInput.
		if(raycasting)
			rayInput = (newinputSteer1 + newinputSteer2 + newinputSteer3 + newinputSteer4 + newinputSteer5 + newinputSteer6);
		else
			rayInput = 0f;

		// If rayInput is too much, ignore navigator input.
		if(raycasting && Mathf.Abs(rayInput) > .5f)
			ignoreWaypointNow = true;
		else
			ignoreWaypointNow = false;
		
	}

	void FeedRCC(){

		// Feeding gasInput of the RCC.
		if(carController.direction == 1){
			if(!limitSpeed){
				carController.gasInput = gasInput;
			}else{
				carController.gasInput = gasInput * Mathf.Clamp01(Mathf.Lerp(10f, 0f, (carController.speed) / maximumSpeed));
			}
		}else{
			carController.gasInput = 0f;
		}

		// Feeding steerInput of the RCC.
		if(smoothedSteer)
			carController.steerInput = Mathf.Lerp(carController.steerInput, steerInput, Time.deltaTime * 20f);
		else
			carController.steerInput = steerInput;

		// Feeding brakeInput of the RCC.
		if(carController.direction == 1)
			carController.brakeInput = brakeInput;
		else
			carController.brakeInput = gasInput;

	}
	
	void OnTriggerEnter (Collider col){

		// Checking if car is in any brake zone on scene.
		if(col.gameObject.GetComponent<RCC_AIBrakeZone>()){
			inBrakeZone = true;
			maximumSpeedInBrakeZone = col.gameObject.GetComponent<RCC_AIBrakeZone>().targetSpeed;
		}
		
	}
	
	void OnTriggerExit (Collider col){
		
		if(col.gameObject.GetComponent<RCC_AIBrakeZone>()){
			inBrakeZone = false;
			maximumSpeedInBrakeZone = 0;
		}
		
	}
	
}