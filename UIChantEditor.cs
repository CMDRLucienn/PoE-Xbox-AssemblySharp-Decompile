using UnityEngine;

public class UIChantEditor : UIHudWindow, ISelectACharacterMutable, ISelectACharacter
{
	public Chant EmptyChantPrefab;

	private CharacterStats m_LoadedCharacter;

	public Color NoteColor;

	public Color RecitationColor;

	public Color LingerColor;

	public static UIChantEditor Instance { get; private set; }

	public CharacterStats SelectedCharacter
	{
		get
		{
			return m_LoadedCharacter;
		}
		set
		{
			m_LoadedCharacter = value;
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(m_LoadedCharacter);
			}
		}
	}

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (WindowActive() || !GameInput.GetControlUp(MappedControl.EDIT_SPELLS, handle: false))
		{
			return;
		}
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		if ((bool)selectedForBars)
		{
			CharacterStats component = selectedForBars.GetComponent<CharacterStats>();
			if ((bool)component && component.CharacterClass == CharacterStats.Class.Chanter)
			{
				ShowWindow();
			}
		}
	}

	public override void HandleInput()
	{
		if (GameInput.GetControlUp(MappedControl.EDIT_SPELLS))
		{
			HideWindow();
		}
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

	protected override void Show()
	{
		if (!(SelectedCharacter == null))
		{
			return;
		}
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		if (!selectedForBars)
		{
			GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
			foreach (GameObject gameObject in selectedPartyMembers)
			{
				if ((bool)gameObject)
				{
					CharacterStats component = gameObject.GetComponent<CharacterStats>();
					if ((bool)component && component.CharacterClass == CharacterStats.Class.Chanter)
					{
						SelectedCharacter = component;
						break;
					}
				}
			}
		}
		else
		{
			CharacterStats component2 = selectedForBars.GetComponent<CharacterStats>();
			if ((bool)component2 && component2.CharacterClass == CharacterStats.Class.Chanter)
			{
				SelectedCharacter = component2;
			}
		}
		if (SelectedCharacter == null)
		{
			HideWindow();
		}
	}

	protected override bool Hide(bool forced)
	{
		SelectedCharacter = null;
		return base.Hide(forced);
	}

	public Color GetColor(UIChantEditorGetColor.ChantColor color)
	{
		return color switch
		{
			UIChantEditorGetColor.ChantColor.LINGER => LingerColor, 
			UIChantEditorGetColor.ChantColor.NOTE => NoteColor, 
			UIChantEditorGetColor.ChantColor.RECITATION => RecitationColor, 
			_ => Color.white, 
		};
	}
}
