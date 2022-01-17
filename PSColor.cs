using UnityEngine;

public class PSColor : MonoBehaviour
{
	private void OnSetColor(Color color)
	{
		GetComponent<ParticleSystem>().GetComponent<Renderer>().material.SetColor("_TintColor", color);
	}

	private void OnGetColor(ColorPicker picker)
	{
		picker.NotifyColor(GetComponent<ParticleSystem>().GetComponent<Renderer>().material.GetColor("_TintColor"));
	}
}
