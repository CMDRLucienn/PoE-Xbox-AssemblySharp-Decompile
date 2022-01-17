using UnityEngine;

public class OnAwakeDeactivate : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.SetActive(value: false);
		GameUtilities.Destroy(this);
	}
}
