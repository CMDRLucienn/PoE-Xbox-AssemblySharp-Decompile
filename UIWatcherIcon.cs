using System;
using UnityEngine;

public class UIWatcherIcon : MonoBehaviour
{
	private GameObject m_Target;

	private SkinnedMeshRenderer m_MeshRenderer;

	private UISprite m_Sprite;

	private UIAnchorToWorld m_Anchor;

	public GameCursor.CursorType MouseOverCursor = GameCursor.CursorType.Interact;

	public GameObject Target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
			FindMeshRenderer();
		}
	}

	private void Start()
	{
		m_Anchor = GetComponent<UIAnchorToWorld>();
		m_Sprite = GetComponent<UISprite>();
		UIEventListener uIEventListener = UIEventListener.Get(m_Sprite.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnIconClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(m_Sprite.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnIconHover));
		m_Sprite.MakePixelPerfect();
	}

	private void OnDestroy()
	{
		UIEventListener uIEventListener = UIEventListener.Get(m_Sprite.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnIconClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(m_Sprite.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Remove(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnIconHover));
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnIconClick(GameObject obj)
	{
		if (m_Sprite.alpha > 0f)
		{
			UIItemInspectManager.Examine(m_Target.GetComponent<BackerContent>());
			GameInput.HandleAllClicks();
		}
	}

	private void OnIconHover(GameObject obj, bool isOver)
	{
		if (isOver && m_Sprite.alpha > 0f)
		{
			GameCursor.UiObjectUnderCursor = base.gameObject;
			GameCursor.CursorOverride = MouseOverCursor;
		}
		else if (GameCursor.UiObjectUnderCursor == base.gameObject)
		{
			GameCursor.UiObjectUnderCursor = null;
			GameCursor.CursorOverride = GameCursor.CursorType.None;
		}
	}

	private void FindMeshRenderer()
	{
		if (!m_Target)
		{
			return;
		}
		SkinnedMeshRenderer[] componentsInChildren = m_Target.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (skinnedMeshRenderer.name == "Mesh")
			{
				m_MeshRenderer = skinnedMeshRenderer;
				break;
			}
		}
	}

	private void Update()
	{
		if ((bool)m_MeshRenderer && (bool)Camera.main)
		{
			Vector3 vector = Camera.main.WorldToScreenPoint(m_MeshRenderer.bounds.center);
			vector = new Vector3(vector.x, vector.y, 0f);
			Vector3 vector2 = Camera.main.WorldToScreenPoint(m_MeshRenderer.bounds.center + m_MeshRenderer.bounds.extents);
			vector2 = new Vector3(vector2.x, vector2.y, 0f);
			float sqrMagnitude = ((vector - vector2) * 4f).sqrMagnitude;
			if (((Input.mousePosition - vector).sqrMagnitude <= sqrMagnitude || InGameHUD.Instance.HighlightActive) && (FogOfWar.Instance == null || FogOfWar.Instance.PointVisible(m_Target.transform.position)))
			{
				m_Sprite.alpha = Mathf.Min(1f, m_Sprite.alpha + 2.25f * Time.deltaTime);
			}
			else
			{
				m_Sprite.alpha = Mathf.Max(0f, m_Sprite.alpha - 2.25f * Time.deltaTime);
			}
			if (m_Sprite.alpha > 0f)
			{
				m_Anchor.AnchorPos = m_MeshRenderer.bounds.center;
				m_Anchor.UpdatePosition();
			}
		}
		else
		{
			FindMeshRenderer();
		}
	}
}
