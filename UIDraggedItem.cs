using UnityEngine;

public class UIDraggedItem : MonoBehaviour
{
	public UIPanel PanelDraggedItem;

	public UITexture TextureItem;

	public UILabel LblQty;

	public GameObject ParentQty;

	private int m_lastUpdateQty;

	private InventoryItem m_draggingItem;

	private UIAnchorToMouse m_MouseAnchor;

	private static UIDraggedItem m_Instance;

	public static UIDraggedItem Instance => m_Instance;

	private void Awake()
	{
		m_Instance = this;
	}

	private void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		if (m_draggingItem != null && m_draggingItem.stackSize != m_lastUpdateQty)
		{
			LblQty.text = m_draggingItem.stackSize.ToString();
			m_lastUpdateQty = m_draggingItem.stackSize;
		}
	}

	private void Init()
	{
		if (!TextureItem)
		{
			TextureItem = GetComponent<UITexture>();
		}
		if (!m_MouseAnchor)
		{
			m_MouseAnchor = base.gameObject.AddComponent<UIAnchorToMouse>();
		}
		ParentQty?.SetActive(value: false);
	}

	public void LoadInventoryItem(InventoryItem draggingItem)
	{
		if (!(TextureItem == null))
		{
			if (draggingItem == null || draggingItem.baseItem == null)
			{
				SetVisible(isVisible: false);
			}
			m_draggingItem = draggingItem;
			TextureItem.mainTexture = draggingItem.baseItem.GetIconLargeTexture();
			TextureItem.MakePixelPerfect();
			int stackSize = draggingItem.stackSize;
			ParentQty.SetActive(stackSize > 1);
			if (stackSize > 1)
			{
				LblQty.text = stackSize.ToString();
			}
			m_lastUpdateQty = stackSize;
		}
	}

	public void SetVisible(bool isVisible)
	{
		Init();
		PanelDraggedItem.gameObject.SetActive(isVisible);
		if ((bool)m_MouseAnchor)
		{
			m_MouseAnchor.Update();
		}
	}
}
