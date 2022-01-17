using UnityEngine;

public class UIRotateByTime : MonoBehaviour
{
	public float ZeroAngle = 90f;

	public int Direction = -1;

	private void Update()
	{
		float num = (float)WorldTime.Instance.TotalSecondsToday / (float)WorldTime.Instance.SecondsPerDay;
		base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num * Mathf.Sign(Direction) * 360f + ZeroAngle));
	}
}
