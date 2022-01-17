using UnityEngine;

[RequireComponent(typeof(UITweener))]
public class ResetTweenOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		GetComponent<UITweener>().Reset();
	}
}
