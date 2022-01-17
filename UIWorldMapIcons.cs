using UnityEngine;

[ExecuteInEditMode]
public class UIWorldMapIcons : MonoBehaviour
{
	public UIWorldMapIcon BaseIcon;

	public GameObject DisableParent;

	public UIWorldMapLinks LinkManager;

	public UIAtlas MapAtlas;

	public UIAtlas MapLockedAtlas;

	public UIWidget CurrentLocationSprite;

	private UIWorldMapIcon[] m_Icons;

	public string MapTag = "world";

	public GUIDatabaseString MapTitle = new GUIDatabaseString();

	public void LoadMapData()
	{
		UIWorldMapIcon[] icons = GetIcons();
		for (int i = 0; i < icons.Length; i++)
		{
			icons[i].Init();
		}
	}

	public UIWorldMapIcon[] GetIcons()
	{
		if (m_Icons == null)
		{
			m_Icons = GetComponentsInChildren<UIWorldMapIcon>(includeInactive: true);
		}
		return m_Icons;
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		UIWorldMapIcon uIWorldMapIcon = null;
		MapData currentMap = UIWorldMapManager.Instance.GetCurrentMap();
		bool flag = InGameHUD.MapTag == "odnua";
		if (currentMap != null)
		{
			uIWorldMapIcon = IconForScene(currentMap.SceneName, flag);
		}
		if ((bool)CurrentLocationSprite)
		{
			UIAnchor component = CurrentLocationSprite.GetComponent<UIAnchor>();
			if ((bool)component)
			{
				component.widgetContainer = null;
				if (uIWorldMapIcon != null)
				{
					component.widgetContainer = uIWorldMapIcon.Widget;
				}
				CurrentLocationSprite.gameObject.SetActive(component.widgetContainer != null);
			}
		}
		if (flag)
		{
			UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
			Vector3 worldPosOfLowestButtonOfOdNua = GetWorldPosOfLowestButtonOfOdNua();
			uIDraggablePanel.MoveAbsolute(new Vector3(0f - worldPosOfLowestButtonOfOdNua.x, 0f - worldPosOfLowestButtonOfOdNua.y, 0f));
			uIDraggablePanel.RestrictWithinBounds(instant: true);
			uIDraggablePanel.MoveRelative(new Vector3(250f, 0f, 0f));
			uIDraggablePanel.RestrictWithinBounds(instant: true);
		}
		else if ((bool)uIWorldMapIcon)
		{
			UIDraggablePanel uIDraggablePanel2 = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
			uIDraggablePanel2.MoveAbsolute(new Vector3(0f - uIWorldMapIcon.transform.position.x, 0f - uIWorldMapIcon.transform.position.y, 0f));
			uIDraggablePanel2.RestrictWithinBounds(instant: true);
		}
	}

	public Vector3 GetWorldPosOfLowestButtonOfOdNua()
	{
		UiOdNuaExternalIcon uiOdNuaExternalIcon = null;
		UiOdNuaExternalIcon[] componentsInChildren = GetComponentsInChildren<UiOdNuaExternalIcon>(includeInactive: true);
		foreach (UiOdNuaExternalIcon uiOdNuaExternalIcon2 in componentsInChildren)
		{
			if (uiOdNuaExternalIcon2.Link.gameObject.activeInHierarchy && (uiOdNuaExternalIcon == null || uiOdNuaExternalIcon2.transform.position.y < uiOdNuaExternalIcon.transform.position.y))
			{
				uiOdNuaExternalIcon = uiOdNuaExternalIcon2;
			}
		}
		if (!(uiOdNuaExternalIcon == null))
		{
			return uiOdNuaExternalIcon.transform.position;
		}
		return Vector3.zero;
	}

	public UIWorldMapIcon IconForScene(string scene, bool getLowestOdNua)
	{
		UIWorldMapIcon[] icons = GetIcons();
		foreach (UIWorldMapIcon uIWorldMapIcon in icons)
		{
			MapData data = uIWorldMapIcon.GetData();
			if (string.Compare(uIWorldMapIcon.gameObject.name, scene, ignoreCase: true) == 0 && data != null && data.MapTag == InGameHUD.MapTag)
			{
				return uIWorldMapIcon;
			}
		}
		return null;
	}

	public void UpdateVisibility()
	{
		UIWorldMapIcon[] icons = GetIcons();
		for (int i = 0; i < icons.Length; i++)
		{
			icons[i].UpdateVisibility();
		}
	}
}
