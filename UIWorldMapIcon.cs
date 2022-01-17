using System;
using UnityEngine;

public class UIWorldMapIcon : MonoBehaviour
{
	public string VirtualForTag = "";

	public string m_MapName;

	private MapData m_Data;

	public GUIDatabaseString OverrideDisplayName = new GUIDatabaseString();

	public UILabel Label;

	public UISprite Sprite;

	public UITexture Texture;

	public Collider ExtraCollider;

	private GameObject m_Collider;

	private UIAbsoluteAnchor m_TexturePos;

	private bool m_Hovered;

	[Range(-1f, 1f)]
	public float LabelOffsetX;

	[Range(-2f, 2f)]
	public float LabelOffsetY = -1f;

	public UIWidget Widget
	{
		get
		{
			if (!Sprite)
			{
				return Texture;
			}
			return Sprite;
		}
	}

	public bool CanTravel
	{
		get
		{
			if (!InGameHUD.TravelEnabled)
			{
				return !string.IsNullOrEmpty(VirtualForTag);
			}
			return true;
		}
	}

	public MapData GetData()
	{
		return m_Data;
	}

	public string GetName()
	{
		return m_MapName;
	}

	public void Init()
	{
		if (!string.IsNullOrEmpty(m_MapName))
		{
			m_Data = WorldMap.Instance.GetMap(m_MapName);
		}
		RefreshName();
	}

	private void OnLanguageChanged(Language lang)
	{
		RefreshName();
	}

	public void RefreshName()
	{
		if ((bool)Label)
		{
			if (OverrideDisplayName.IsValidString)
			{
				Label.text = OverrideDisplayName.GetText();
			}
			else if (m_Data != null)
			{
				Label.text = m_Data.DisplayName.GetText();
			}
			else
			{
				Label.text = "";
			}
		}
	}

	private void OnEnable()
	{
		if ((bool)Widget)
		{
			UIImageButtonRevised component = Widget.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.enabled = CanTravel;
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)Widget)
		{
			UIImageButtonRevised component = Widget.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		if (m_TexturePos != null)
		{
			m_TexturePos.LocalPosition = Vector3.zero;
		}
	}

	private void Start()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		if ((bool)Sprite)
		{
			m_Collider = new GameObject("Collider");
			Transform obj = m_Collider.transform;
			obj.parent = Sprite.transform.parent;
			obj.localPosition = Sprite.transform.localPosition;
			obj.localRotation = Quaternion.identity;
			obj.localScale = Sprite.transform.localScale;
			m_Collider.layer = base.gameObject.layer;
			m_Collider.AddComponent(typeof(BoxCollider));
			m_Collider.AddComponent(typeof(UIDragPanelContents));
			UIImageButtonRevised[] componentsInChildren = Sprite.transform.parent.GetComponentsInChildren<UIImageButtonRevised>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].AddAdditionalCollider(m_Collider);
			}
			if (Sprite.transform.parent != base.transform)
			{
				componentsInChildren = GetComponentsInChildren<UIImageButtonRevised>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].AddAdditionalCollider(m_Collider);
				}
			}
			UIEventListener uIEventListener = UIEventListener.Get(m_Collider);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
			UIEventListener uIEventListener2 = UIEventListener.Get(m_Collider);
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnHover));
		}
		if ((bool)ExtraCollider)
		{
			UIImageButtonRevised[] componentsInChildren = Sprite.transform.parent.GetComponentsInChildren<UIImageButtonRevised>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].AddAdditionalCollider(ExtraCollider.gameObject);
			}
			if (Sprite.transform.parent != base.transform)
			{
				componentsInChildren = GetComponentsInChildren<UIImageButtonRevised>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].AddAdditionalCollider(ExtraCollider.gameObject);
				}
			}
			UIEventListener uIEventListener3 = UIEventListener.Get(ExtraCollider);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnClick));
			UIEventListener uIEventListener4 = UIEventListener.Get(ExtraCollider);
			uIEventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onHover, new UIEventListener.BoolDelegate(OnHover));
		}
		m_TexturePos = Widget.GetComponent<UIAbsoluteAnchor>();
		UpdateLabelPosition();
	}

	private void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
	}

	private void Update()
	{
		if (m_Data != null && (bool)m_Collider)
		{
			bool flag = string.IsNullOrEmpty(VirtualForTag) && m_Data == UIWorldMapManager.Instance.GetCurrentMap();
			m_Collider.SetActive(m_Data.Visibility != 0 && (bool)UIWorldMapManager.Instance && !flag);
		}
	}

	public void UpdateVisibility()
	{
		bool active = m_Data == null || m_Data.IsVisibleToUser;
		base.gameObject.SetActive(active);
		if (!Sprite)
		{
			return;
		}
		UIWorldMapIcons componentInParent = base.transform.GetComponentInParent<UIWorldMapIcons>();
		if ((bool)componentInParent)
		{
			if (m_Data != null && m_Data.Visibility == MapData.VisibilityType.Locked)
			{
				Sprite.atlas = componentInParent.MapLockedAtlas;
			}
			else
			{
				Sprite.atlas = componentInParent.MapAtlas;
			}
		}
	}

	private void OnClick(GameObject go)
	{
		if (!string.IsNullOrEmpty(VirtualForTag))
		{
			InGameHUD.MapTag = VirtualForTag;
			UIWorldMapManager.Instance.ReloadMapTag();
		}
		else if (CanTravel && m_Data != null && UIWorldMapManager.Instance.GetCurrentMap() != m_Data)
		{
			if (UIWorldMapManager.Instance.MasterLinks.GetConnected(UIWorldMapManager.Instance.GetCurrentMap(), m_Data))
			{
				EternityTimeInterval eternityTimeInterval = UIWorldMapManager.Instance.MasterLinks.TravelTimeTo(m_Data);
				UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, m_Data.DisplayName.GetText(), GUIUtils.Format(263, eternityTimeInterval.FormatNonZero(1), UIWorldMapManager.Instance.GetCurrentMap().DisplayName.GetText(), m_Data.DisplayName.GetText()));
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnConfirmDialog));
			}
			else
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, m_Data.DisplayName.GetText(), GUIUtils.Format(271, m_Data.DisplayName.GetText(), UIWorldMapManager.Instance.GetCurrentMap().DisplayName.GetText()));
			}
		}
	}

	private void TransitionOnEndFade()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(TransitionOnEndFade));
		UIWorldMapIcons componentInParent = GetComponentInParent<UIWorldMapIcons>();
		if ((bool)componentInParent)
		{
			UIWorldMapManager.Instance.DoTransition(m_MapName, componentInParent.MapTag);
		}
		else
		{
			Debug.LogError("WorldMapIcon '" + base.name + "' found no owner.");
		}
	}

	private void OnConfirmDialog(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE && !FadeManager.Instance.IsFadeActive())
		{
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(TransitionOnEndFade));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0.35f, AudioFadeMode.Fx);
		}
	}

	private void OnHover(GameObject go, bool over)
	{
		if (CanTravel && (bool)m_TexturePos)
		{
			if (over && !m_Hovered)
			{
				m_TexturePos.LocalPosition += (Vector3)UIWorldMapManager.Instance.IconMouseoverOffset;
			}
			else if (!over && m_Hovered)
			{
				m_TexturePos.LocalPosition -= (Vector3)UIWorldMapManager.Instance.IconMouseoverOffset;
			}
		}
		m_Hovered = over;
	}

	public void SetMap(MapData data)
	{
		m_Data = data;
		m_MapName = data.SceneName;
		UILabel componentInChildren = GetComponentInChildren<UILabel>();
		if ((bool)componentInChildren)
		{
			componentInChildren.text = data.SceneName;
		}
	}

	public void UpdateLabelPosition()
	{
		if ((bool)Label)
		{
			Vector3 vector = new Vector3((Widget.transform.localScale.x + Label.relativeSize.x * Label.transform.localScale.x) * LabelOffsetX / 2f, (Widget.transform.localScale.y + Label.relativeSize.y * Label.transform.localScale.y) * LabelOffsetY / 2f, Label.transform.localPosition.z);
			if (vector != Label.transform.localPosition)
			{
				Label.transform.localPosition = vector;
			}
		}
	}
}
