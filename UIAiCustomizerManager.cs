using System;
using UnityEngine;

public class UIAiCustomizerManager : UIHudWindow, ISelectACharacter
{
	public UICharacterAiScriptDropdown AiPackageDropdown;

	public UICharacterAutoAttackDropdown AutoAttackDropdown;

	public UICharacterAutoAttackDropdown CompanionAutoAttackDropdown;

	public UICharacterAiUseRestCheckbox UseRestDropdown;

	public UIMultiSpriteImageButton ButtonSave;

	public UIMultiSpriteImageButton ButtonCancel;

	public UILabel ScriptDescLabel;

	public UIDraggablePanel ScriptDescPanel;

	public UIWidget Background;

	private CharacterStats m_selectedCharacter;

	private PartyMemberAI m_selectedPai;

	public static UIAiCustomizerManager Instance { get; private set; }

	public CharacterStats SelectedCharacter
	{
		get
		{
			return m_selectedCharacter;
		}
		set
		{
			m_selectedCharacter = value;
			m_selectedPai = (m_selectedCharacter ? m_selectedCharacter.GetComponent<PartyMemberAI>() : null);
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(m_selectedCharacter);
			}
		}
	}

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		UIMultiSpriteImageButton buttonSave = ButtonSave;
		buttonSave.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(buttonSave.onClick, new UIEventListener.VoidDelegate(OnSave));
		UIMultiSpriteImageButton buttonCancel = ButtonCancel;
		buttonCancel.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(buttonCancel.onClick, new UIEventListener.VoidDelegate(OnCancel));
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnSave(GameObject sender)
	{
		if ((bool)m_selectedPai)
		{
			if (AiPackageDropdown.Setting >= 0)
			{
				m_selectedPai.SetInstructionSetIndex(AiPackageDropdown.Setting);
			}
			else
			{
				m_selectedPai.ClearInstructionSet();
			}
			m_selectedPai.Aggression = AutoAttackDropdown.Setting;
			m_selectedPai.UpdateAggressionOfSummonedCreatures(includeCompanion: false);
			m_selectedPai.UsePerRestAbilitiesInInstructionSet = UseRestDropdown.Setting;
			GameObject gameObject = GameUtilities.FindAnimalCompanion(m_selectedPai.gameObject);
			if ((bool)gameObject)
			{
				AIController aIController = GameUtilities.FindActiveAIController(gameObject);
				if ((bool)aIController)
				{
					aIController.Aggression = CompanionAutoAttackDropdown.Setting;
					if (aIController is PartyMemberAI)
					{
						(aIController as PartyMemberAI).UsePerRestAbilitiesInInstructionSet = UseRestDropdown.Setting;
					}
				}
			}
		}
		HideWindow();
	}

	private void OnCancel(GameObject sender)
	{
		HideWindow();
	}

	protected override void Show()
	{
		base.Show();
	}

	public void SetScriptTooltip(string text)
	{
		ScriptDescLabel.text = text;
		ScriptDescPanel.ResetPosition();
	}
}
