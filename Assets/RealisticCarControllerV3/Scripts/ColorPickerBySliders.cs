using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorPickerBySliders : MonoBehaviour {

	public Color color;		// Main color.

	// Sliders per color channel.
	public Slider redSlider;
	public Slider greenSlider;
	public Slider blueSlider;

	public void Update () {

		color = new Color (redSlider.value, greenSlider.value, blueSlider.value);
	
	}

}
