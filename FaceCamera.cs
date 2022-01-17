using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	private void Start()
	{
		base.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
	}
}
