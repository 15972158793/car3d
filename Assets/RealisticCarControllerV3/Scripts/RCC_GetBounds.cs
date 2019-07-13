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
/// Getting total bound size of a gameobject.
/// </summary>
public class RCC_GetBounds : MonoBehaviour {
	
	public static Vector3 GetBoundsCenter(Transform obj){
		
		// get the maximum bounds extent of object, including all child renderers,
		// but excluding particles and trails, for FOV zooming effect.

		var renderers = obj.GetComponentsInChildren<Renderer>();

		Bounds bounds = new Bounds();
		bool initBounds = false;
		foreach (Renderer r in renderers){
			
			if (!((r is TrailRenderer) || (r is ParticleRenderer) || (r is ParticleSystemRenderer))){
				
				if (!initBounds){
					initBounds = true;
					bounds = r.bounds;
				}else{
					bounds.Encapsulate(r.bounds);
				}
			}

		}

		Vector3 center = bounds.center;
		return center;

	}

}
