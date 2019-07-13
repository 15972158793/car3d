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
/// Used for changing wheels (visual only) at runtime. It holds changable wheels as prefab in an array.
/// </summary>
[System.Serializable]
public class RCC_ChangableWheels : ScriptableObject {
	
	#region singleton
	public static RCC_ChangableWheels instance;
	public static RCC_ChangableWheels Instance{	get{if(instance == null) instance = Resources.Load("RCCAssets/RCC_ChangableWheels") as RCC_ChangableWheels; return instance;}}
	#endregion

	[System.Serializable]
	public class ChangableWheels{
		
		public GameObject wheel;

	}
		
	public ChangableWheels[] wheels;

}


