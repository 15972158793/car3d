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

/// <summary>
/// Fixed camera system for RCC Camera. It simply parents the RCC Camera, and calculates target position, rotation, FOV, etc...
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Camera/Fixed Camera")]
public class RCC_FixedCamera : MonoBehaviour {

	public Transform currentCar;
	private RCC_Camera rccCamera;

	private Vector3 targetPosition;
	private Vector3 smoothedPosition;
	public float maxDistance = 50f;
	private float distance;

	public float minimumFOV = 20f;
	public float maximumFOV = 60f;
	public bool canTrackNow = false;

	private float timer = 0f;
	public float updateInSeconds = .05f;

	void Start(){

		rccCamera = GameObject.FindObjectOfType<RCC_Camera> ();

	}

	void LateUpdate(){

		if (!canTrackNow)
			return;

		if (!rccCamera) {
			rccCamera = GameObject.FindObjectOfType<RCC_Camera> ();
			return;
		}

		if (!currentCar) {
			currentCar = rccCamera.playerCar;
			return;
		}

		CheckCulling ();
			
		targetPosition = currentCar.position;
		targetPosition += currentCar.rotation * Vector3.forward * (currentCar.GetComponent<RCC_CarControllerV3>().speed * .05f);

		smoothedPosition = Vector3.Lerp (smoothedPosition, targetPosition, Time.deltaTime * 5f);

		distance = Vector3.Distance (transform.position, currentCar.position);
		rccCamera.targetFieldOfView = Mathf.Lerp (distance > maxDistance / 10f ? maximumFOV : 70f, minimumFOV, (distance * 1.5f) / maxDistance);

		transform.LookAt (smoothedPosition);

		transform.Translate ((-currentCar.forward * currentCar.GetComponent<RCC_CarControllerV3>().speed) / 50f * Time.deltaTime);

	}

	void CheckCulling(){

		timer += Time.deltaTime;

		if (timer < updateInSeconds) {
			return;
		} else {
			timer = 0f;
		}
			
		RaycastHit hit;

		if ((Physics.Linecast (currentCar.position, transform.position, out hit) && !hit.transform.IsChildOf (currentCar) && !hit.collider.isTrigger) || distance >= maxDistance) {
			ChangePosition ();
		}

	}

	void ChangePosition(){

		float randomizedAngle = Random.Range (-15f, 15f);
		RaycastHit hit;

		if (Physics.Raycast (currentCar.position, Quaternion.AngleAxis (randomizedAngle, currentCar.up) * currentCar.forward, out hit, maxDistance) && !hit.transform.IsChildOf(currentCar) && !hit.collider.isTrigger) {
			transform.position = hit.point;
			transform.LookAt (currentCar.position + new Vector3(0f, Mathf.Clamp(randomizedAngle, .5f, 5f), 0f));
			transform.position += transform.rotation * Vector3.forward * 5f;
		} else {
			transform.position = currentCar.position + new Vector3(0f, Mathf.Clamp(randomizedAngle, 0f, 5f), 0f);
			transform.position += Quaternion.AngleAxis (randomizedAngle, currentCar.up) * currentCar.forward * (maxDistance * .9f);
		}

	}
	
}
