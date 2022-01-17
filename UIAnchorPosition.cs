using UnityEngine;

[ExecuteInEditMode]
public class UIAnchorPosition : MonoBehaviour
{
	public Vector3 Position;

	public bool OnX = true;

	public bool OnY = true;

	private void Update()
	{
		if (OnX)
		{
			base.transform.localPosition = new Vector3(Position.x, OnY ? Position.y : base.transform.localPosition.y, base.transform.localPosition.z);
		}
		else if (OnY)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, Position.y, base.transform.localPosition.z);
		}
	}
}
