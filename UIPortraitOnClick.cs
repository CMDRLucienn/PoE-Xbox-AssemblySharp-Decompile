using UnityEngine;

public class UIPortraitOnClick : UIParentSelectorListener
{
	private UIPartyPortrait m_Portrait;

	private UIPartyPortraitBar m_Owner;

	public bool AllowDrag = true;

	private static UIPortraitOnClick s_Dragging;

	private float m_DraggedX;

	private bool m_Hovered;

	protected override void Start()
	{
		m_Portrait = GetComponentInParent<UIPartyPortrait>();
		m_Owner = UIPartyPortraitBar.Instance;
		base.Start();
	}

	protected override void OnDestroy()
	{
		if (s_Dragging == this)
		{
			s_Dragging = null;
		}
		base.OnDestroy();
	}

	private void Update()
	{
		if (m_Hovered && (bool)ParentSelector.SelectedCharacter)
		{
			GameCursor.OverrideCharacterUnderCursor = ParentSelector.SelectedCharacter.gameObject;
		}
		else if (!ParentSelector.SelectedCharacter || GameCursor.OverrideCharacterUnderCursor == ParentSelector.SelectedCharacter.gameObject)
		{
			GameCursor.OverrideCharacterUnderCursor = null;
		}
	}

	private void OnHover(bool over)
	{
		m_Hovered = over;
	}

	private void OnDrag(Vector2 delta)
	{
		if (AllowDrag)
		{
			if (s_Dragging == null)
			{
				s_Dragging = this;
				m_Portrait.Grab();
			}
			m_DraggedX += delta.x;
			if (Mathf.Abs(m_DraggedX) > m_Owner.PortraitWidth / 2f)
			{
				m_Owner.ShiftPortrait(base.transform.parent.gameObject, (int)Mathf.Sign(m_DraggedX));
				m_DraggedX -= Mathf.Sign(m_DraggedX) * m_Owner.PortraitWidth;
			}
		}
	}

	private void OnRightClick()
	{
		CharacterStats selectedCharacter = ParentSelector.SelectedCharacter;
		AIController aIController = (selectedCharacter ? GameUtilities.FindActiveAIController(selectedCharacter.gameObject) : null);
		if ((bool)aIController && aIController.SummonType == AIController.AISummonType.NotSummoned)
		{
			UIInventoryManager.Instance.SelectPartyMember(aIController);
			UIInventoryManager.Instance.ShowWindow();
		}
	}

	private void OnPress(bool down)
	{
		if (AllowDrag)
		{
			if (down)
			{
				Vector3 point = InGameUILayout.NGUICamera.ScreenToWorldPoint(GameInput.MousePosition);
				m_DraggedX = base.transform.worldToLocalMatrix.MultiplyPoint3x4(point).x;
				m_Owner.StartDrag();
			}
			else if (s_Dragging == this)
			{
				s_Dragging = null;
				m_Portrait.LetGo();
				m_Owner.EndDrag();
			}
		}
	}
}
