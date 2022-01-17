using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIInventoryItemGridPageLabel : MonoBehaviour
{
	private UILabel m_Label;

	public UIInventoryItemGrid ItemGrid;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
		if (ItemGrid != null)
		{
			ItemGrid.OnPostReload += OnItemGridReloaded;
		}
	}

	private void OnDestroy()
	{
		if (ItemGrid != null)
		{
			ItemGrid.OnPostReload -= OnItemGridReloaded;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnItemGridReloaded(UIInventoryItemGrid grid)
	{
		if (ItemGrid == grid)
		{
			m_Label.text = GUIUtils.Format(451, ItemGrid.CurrentPage + 1, ItemGrid.PageCount);
		}
	}
}
