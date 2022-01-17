using UnityEngine;

public class IgnoreParentRotation : MonoBehaviour
{
	[HideInInspector]
	public GameObject AttachedChild;

	private void Start()
	{
		AttachedChild = Object.Instantiate(base.gameObject);
		GameUtilities.Destroy(AttachedChild.GetComponent<IgnoreParentRotation>());
		IgnoreParentRotationChild ignoreParentRotationChild = AttachedChild.AddComponent<IgnoreParentRotationChild>();
		if ((bool)ignoreParentRotationChild)
		{
			ignoreParentRotationChild.AttachedParent = base.gameObject;
		}
		for (int num = base.gameObject.transform.childCount - 1; num >= 0; num--)
		{
			GameUtilities.Destroy(base.transform.GetChild(num).gameObject);
		}
	}

	private void OnDisable()
	{
		if ((bool)AttachedChild)
		{
			AttachedChild.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if ((bool)AttachedChild)
		{
			AttachedChild.SetActive(value: true);
		}
	}

	private void OnDestroy()
	{
		GameUtilities.Destroy(AttachedChild);
		AttachedChild = null;
		ComponentUtils.NullOutObjectReferences(this);
	}
}
