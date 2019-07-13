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
/// Will simulate chassis movement based on car rigidbody velocity.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/Chassis")]
public class RCC_Chassis : MonoBehaviour {

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

	private Rigidbody mainRigid;

	private float chassisVerticalLean = 4.0f;		// Chassis Vertical Lean Sensitivity.
	private float chassisHorizontalLean = 4.0f;		// Chassis Horizontal Lean Sensitivity.

	private float horizontalLean = 0f;
	private float verticalLean = 0f;

	void Start () {

		mainRigid = GetComponentInParent<RCC_CarControllerV3> ().GetComponent<Rigidbody> ();

		chassisVerticalLean = GetComponentInParent<RCC_CarControllerV3> ().chassisVerticalLean;
		chassisHorizontalLean = GetComponentInParent<RCC_CarControllerV3> ().chassisHorizontalLean;

		if (!RCCSettings.dontUseChassisJoint)
			ChassisJoint ();

	}

	void OnEnable(){

		if (!RCCSettings.dontUseChassisJoint)
			StartCoroutine ("ReEnable");

	}

	IEnumerator ReEnable(){

		if(!GetComponent<ConfigurableJoint>())
			yield return null;

		GameObject _joint = GetComponentInParent<ConfigurableJoint>().gameObject;

		_joint.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		yield return new WaitForFixedUpdate();
		_joint.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

	}

	void ChassisJoint(){

		GameObject colliders = new GameObject("Colliders");
		colliders.transform.SetParent(GetComponentInParent<RCC_CarControllerV3> ().transform, false);

		GameObject chassisJoint;

		Transform[] childTransforms = GetComponentInParent<RCC_CarControllerV3> ().chassis.GetComponentsInChildren<Transform>();

		foreach(Transform t in childTransforms){

			if(t.gameObject.activeSelf && t.GetComponent<Collider>()){

				if (t.childCount >= 1) {
					Transform[] childObjects = t.GetComponentsInChildren<Transform> ();
					foreach (Transform c in childObjects) {
						if (c != t) {
							c.SetParent (transform);
						}
					}
				}

				GameObject newGO = (GameObject)Instantiate(t.gameObject, t.transform.position, t.transform.rotation);
				newGO.transform.SetParent(colliders.transform, true);
				newGO.transform.localScale = t.lossyScale;

				Component[] components = newGO.GetComponents(typeof(Component));

				foreach(Component comp  in components){
					if(!(comp is Transform) && !(comp is Collider)){
						Destroy(comp);
					}
				}

			}

		}

		chassisJoint = (GameObject)Instantiate((RCCSettings.chassisJoint), Vector3.zero, Quaternion.identity);
		chassisJoint.transform.SetParent(mainRigid.transform, false);
		chassisJoint.GetComponent<ConfigurableJoint> ().connectedBody = mainRigid;
		chassisJoint.GetComponent<ConfigurableJoint> ().autoConfigureConnectedAnchor = false;

		transform.SetParent(chassisJoint.transform, false);

		Collider[] collidersInChassis = GetComponentsInChildren<Collider>();

		foreach(Collider c in collidersInChassis)
			Destroy(c);

		GetComponentInParent<Rigidbody> ().centerOfMass = new Vector3 (mainRigid.centerOfMass.x, mainRigid.centerOfMass.y + 1f, mainRigid.centerOfMass.z);

	}

	void FixedUpdate () {

		if (RCCSettings.dontUseChassisJoint)
			LegacyChassis ();

	}

	void LegacyChassis (){

		verticalLean = Mathf.Clamp(Mathf.Lerp (verticalLean, mainRigid.angularVelocity.x * chassisVerticalLean, Time.fixedDeltaTime * 5f), -3f, 3f);
		horizontalLean = Mathf.Clamp(Mathf.Lerp (horizontalLean, (transform.InverseTransformDirection(mainRigid.angularVelocity).y * (transform.InverseTransformDirection(mainRigid.velocity).z >= 0 ? 1 : -1)) * chassisHorizontalLean, Time.fixedDeltaTime * 5f), -3f, 3f);

		if(float.IsNaN(verticalLean) || float.IsNaN(horizontalLean) || float.IsInfinity(verticalLean) || float.IsInfinity(horizontalLean) || Mathf.Approximately(verticalLean, 0f) || Mathf.Approximately(horizontalLean, 0f))
			return;

		Quaternion target = Quaternion.Euler(verticalLean, transform.localRotation.y + (mainRigid.angularVelocity.z), horizontalLean);
		transform.localRotation = target;

	}

}
