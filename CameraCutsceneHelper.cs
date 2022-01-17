using UnityEngine;

public class CameraCutsceneHelper : MonoBehaviour
{
	public Transform SceneFocus;

	public GameObject Weapon;

	public GameObject Guy;

	private bool setup;

	private void Start()
	{
		FogOfWarRender.Instance.enabled = false;
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.RevealAll();
		}
		GameObject.Find("DynamicOcclusionPass").SetActive(value: false);
	}

	private void Update()
	{
		if (!setup)
		{
			CameraControl.Instance.FocusOnPoint(SceneFocus.position, 0f);
			CameraControl.Instance.EnablePlayerControl(enableControl: false);
			InGameHUD.Instance.ShowHUD = false;
			Transform boneTransform = GetBoneTransform("primaryWeapon", Guy.transform);
			Object.Instantiate(Weapon).transform.parent = boneTransform;
			setup = true;
		}
	}

	public Transform GetBoneTransform(string name, Transform parent)
	{
		Transform transform = parent.Find(name);
		if (transform != null)
		{
			return transform;
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			transform = parent.GetChild(i).Find(name);
			if (transform != null)
			{
				return transform;
			}
		}
		for (int j = 0; j < childCount; j++)
		{
			transform = GetBoneTransform(name, parent.GetChild(j));
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}
}
