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
/// Exhaust based on Particle System. Based on car controller's throttle situation.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/Exhaust")]
public class RCC_Exhaust : MonoBehaviour {

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

	private RCC_CarControllerV3 carController;
	private ParticleSystem particle;
	private ParticleSystem.EmissionModule emission;
	private ParticleSystem.MinMaxCurve emissionRate;
	public ParticleSystem flame;
	private ParticleSystem.EmissionModule subEmission;
	private ParticleSystem.MinMaxCurve subEmissionRate;
	private Light flameLight;

	public float flameTime = 0f;
	private AudioSource flameSource;

	public Color flameColor = Color.red;
	public Color boostFlameColor = Color.blue;

	public bool previewFlames = false;

	void Start () {

		if (RCCSettings.dontUseAnyParticleEffects) {
			Destroy (gameObject);
			return;
		}

		carController = GetComponentInParent<RCC_CarControllerV3>();
		particle = GetComponent<ParticleSystem>();
		emission = particle.emission;

		if(flame){
			
			subEmission = flame.emission;
			flameLight = flame.GetComponentInChildren<Light>();
			flameSource = RCC_CreateAudioSource.NewAudioSource(gameObject, "Exhaust Flame AudioSource", 10f, 25f, 1f, RCCSettings.exhaustFlameClips[0], false, false, false);
			flameLight.renderMode = RCCSettings.useLightsAsVertexLights ? LightRenderMode.ForceVertex : LightRenderMode.ForcePixel;

		}
	
	}

	void Update () {

		if(!carController || !particle)
			return;

		if(carController.engineRunning){

			if(carController.speed < 150){
				if(!emission.enabled)
					emission.enabled = true;
			if(carController._gasInput > .05f){
				emissionRate.constantMax = 50f;
				emission.rate = emissionRate;
				particle.startSpeed = 5f;
				particle.startSize = 5f;
				//particle.startLifetime = .25f;
			}else{
				emissionRate.constantMax = 5;
				emission.rate = emissionRate;
				particle.startSpeed = .5f;
				particle.startSize = 2.5f;
				//particle.startLifetime = 1f;
				}
			}else{
				if(emission.enabled)
					emission.enabled = false;
			}

			if(carController._gasInput >= .25f)
				flameTime = 0f;

			if(((carController.useExhaustFlame && carController.engineRPM >= 5000 && carController.engineRPM <= 5500 && carController._gasInput <= .25f && flameTime <= .5f) || carController._boostInput >= 1.5f) || previewFlames){
				
				flameTime += Time.deltaTime;
				subEmission.enabled = true;

				if(flameLight)
					flameLight.intensity = flameSource.pitch * 3f * Random.Range(.25f, 1f) ;
				
				if(carController._boostInput >= 1.5f && flame){
					flame.startColor = boostFlameColor;
					flameLight.color = flame.startColor;
				}else{
					flame.startColor = flameColor;
					flameLight.color = flame.startColor;
				}

				if(!flameSource.isPlaying){
					flameSource.clip = RCCSettings.exhaustFlameClips[Random.Range(0, RCCSettings.exhaustFlameClips.Length)];
					flameSource.Play();
				}

			}else{
				
				subEmission.enabled = false;

				if(flameLight)
					flameLight.intensity = 0f;
				if(flameSource.isPlaying)
					flameSource.Stop();
				
			}
				
		}else{

			if(emission.enabled)
				emission.enabled = false;

			subEmission.enabled = false;

			if(flameLight)
				flameLight.intensity = 0f;
			if(flameSource.isPlaying)
				flameSource.Stop();
			
		}

	}

}
