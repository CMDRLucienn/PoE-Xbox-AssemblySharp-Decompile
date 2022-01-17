using UnityEngine;

public class UIAtlasManager : MonoBehaviour
{
	private static UIAtlasManager s_Instance;

	public UIAtlas CommonTinted;

	public UIAtlas Inventory;

	public UIAtlas InventoryTop;

	public UIAtlas InventoryBack;

	public static UIAtlasManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}
}
