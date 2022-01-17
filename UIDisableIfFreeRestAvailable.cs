using System;
using UnityEngine;

public class UIDisableIfFreeRestAvailable : MonoBehaviour
{
	[Tooltip("The object to disable and enable.")]
	public GameObject Target;

	[Tooltip("If set, enable the object instead.")]
	public bool Invert;

	private void Start()
	{
		GameState.OnLevelLoaded += OnLevelLoaded;
		Stronghold instance = Stronghold.Instance;
		instance.OnUpgradeStatusChanged = (Stronghold.UpgradeStatusChanged)Delegate.Combine(instance.OnUpgradeStatusChanged, new Stronghold.UpgradeStatusChanged(OnUpgradeStatusChanged));
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
		if ((bool)Stronghold.Instance)
		{
			Stronghold instance = Stronghold.Instance;
			instance.OnUpgradeStatusChanged = (Stronghold.UpgradeStatusChanged)Delegate.Remove(instance.OnUpgradeStatusChanged, new Stronghold.UpgradeStatusChanged(OnUpgradeStatusChanged));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		Target.SetActive(Invert == FreeRestAvailable());
	}

	private void OnUpgradeStatusChanged(StrongholdUpgrade.Type type)
	{
		Target.SetActive(Invert == FreeRestAvailable());
	}

	public static bool FreeRestAvailable()
	{
		if (GameState.Instance.CurrentMapIsStronghold && Stronghold.Instance.Activated)
		{
			return Stronghold.Instance.HasUpgrade(StrongholdUpgrade.Type.Bedding);
		}
		return false;
	}
}
