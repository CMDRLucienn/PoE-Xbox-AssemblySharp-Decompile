using UnityEngine;

[ExecuteInEditMode]
public class UIAbsoluteAnchor : MonoBehaviour
{
	public Vector3 LocalPosition = Vector3.zero;

	public bool UseEuler;

	public Quaternion LocalRotation = Quaternion.identity;

	public Vector3 LocalEuler = Vector3.zero;

	private void Update()
	{
		if (base.transform.localPosition != LocalPosition)
		{
			base.transform.localPosition = LocalPosition;
		}
		if (UseEuler)
		{
			Quaternion quaternion = Quaternion.Euler(LocalEuler);
			if (base.transform.localRotation != quaternion)
			{
				base.transform.localRotation = quaternion;
			}
		}
		else if (base.transform.localRotation != LocalRotation)
		{
			base.transform.localRotation = LocalRotation;
		}
	}
}
