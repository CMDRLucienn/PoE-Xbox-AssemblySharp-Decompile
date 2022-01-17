using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
	[Persistent]
	public float WaterLevelCurrent;

	[Persistent]
	public float WaterLevelTimer;

	[Persistent]
	public float WaterLevelTotalTime;

	[Persistent]
	public float WaterLevelDesired;

	[Persistent]
	public float WaterLevelPrevious;

	private void Start()
	{
		WaterLevelCurrent = base.transform.position.y;
	}

	public void Restored()
	{
		base.transform.position = new Vector3(base.transform.position.x, WaterLevelCurrent, base.transform.position.z);
	}

	private void Update()
	{
		if (WaterLevelTimer > 0f)
		{
			WaterLevelTimer -= Time.deltaTime;
			if (WaterLevelTimer <= 0f)
			{
				WaterLevelCurrent = WaterLevelDesired;
				base.transform.position = new Vector3(base.transform.position.x, WaterLevelCurrent, base.transform.position.z);
			}
			else
			{
				WaterLevelCurrent = Mathf.SmoothStep(WaterLevelPrevious, WaterLevelDesired, 1f - WaterLevelTimer / WaterLevelTotalTime);
				base.transform.position = new Vector3(base.transform.position.x, WaterLevelCurrent, base.transform.position.z);
			}
		}
	}

	public void ChangeWaterLevel(float newLevel, float time)
	{
		WaterLevelTimer = time;
		WaterLevelTotalTime = time;
		WaterLevelDesired = newLevel;
		WaterLevelPrevious = WaterLevelCurrent;
	}
}
