using System;
using UnityEngine;

public class UILockedDoorTooltip : UIBaseTooltip
{
	private static UILockedDoorTooltip s_Instance;

	public GameObject LockedContent;

	public UILabel DifficultyLabel;

	public GameObject KeyContent;

	public UILabel KeyLabel;

	private UIAnchorToWorld m_WorldAnchor;

	private UILockedDoorOption[] m_Options;

	private OCL m_Target;

	private void Awake()
	{
		s_Instance = this;
		m_WorldAnchor = GetComponent<UIAnchorToWorld>();
		m_Options = GetComponentsInChildren<UILockedDoorOption>(includeInactive: true);
		PartyMemberAI.OnAnySelectionChanged += OnSelectionChanged;
	}

	protected override void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		PartyMemberAI.OnAnySelectionChanged -= OnSelectionChanged;
		base.OnDestroy();
	}

	protected override void Update()
	{
		base.Update();
		if (Panel.alpha > 0f)
		{
			m_WorldAnchor.UpdatePosition();
			base.transform.localPosition = m_WorldAnchor.Position;
		}
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		Reload();
	}

	protected override void SetText(string text)
	{
	}

	public void Set(OCL ocl)
	{
		m_Target = ocl;
		m_WorldAnchor.SetAnchor(m_Target.gameObject);
		Reload();
	}

	private void Reload()
	{
		if (!m_Target)
		{
			Hide();
			return;
		}
		if ((bool)m_Target.Key && PartyHelper.PartyHasItem(m_Target.Key))
		{
			KeyLabel.text = GUIUtils.Format(1732, m_Target.Key.Name);
			KeyLabel.color = Color.white;
			LockedContent.SetActive(value: false);
			KeyContent.SetActive(value: true);
			return;
		}
		if (m_Target.MustHaveKey)
		{
			KeyLabel.text = GUIUtils.Format(414);
			KeyLabel.color = Color.red;
			LockedContent.SetActive(value: false);
			KeyContent.SetActive(value: true);
			return;
		}
		LockedContent.SetActive(value: true);
		KeyContent.SetActive(value: false);
		CharacterStats character = null;
		int num = -1;
		GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
		foreach (GameObject gameObject in selectedPartyMembers)
		{
			if (!gameObject)
			{
				continue;
			}
			CharacterStats component = gameObject.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				int num2 = component.CalculateSkill(CharacterStats.SkillType.Mechanics);
				if (num2 > num)
				{
					num = num2;
					character = component;
				}
			}
		}
		DifficultyLabel.text = GUIUtils.GetText(166) + ": " + m_Target.LockDifficulty;
		for (int j = 0; j < m_Options.Length; j++)
		{
			m_Options[j].Load(character, m_Target);
		}
	}

	public static void GlobalShow(OCL target)
	{
		if (s_Instance != null)
		{
			s_Instance.Show(null, null);
		}
		s_Instance.Set(target);
	}

	public static void GlobalHide()
	{
		if (s_Instance != null)
		{
			s_Instance.Hide();
		}
	}
}
