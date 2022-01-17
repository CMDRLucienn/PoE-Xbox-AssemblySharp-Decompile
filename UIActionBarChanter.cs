using System;
using UnityEngine;

public class UIActionBarChanter : MonoBehaviour
{
	public GameObject Window;

	public UIActionBarChanterDisplay Display;

	public int PixelsPerSecond = 80;

	public int PixelVerticalShift = 12;

	public static UIActionBarChanter Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		PartyMemberAI.OnAnySelectionChanged -= OnSelectionChanged;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		PartyMemberAI.OnAnySelectionChanged += OnSelectionChanged;
		Display.gameObject.SetActive(value: false);
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		if (selectedForBars != null)
		{
			CharacterStats component = selectedForBars.GetComponent<CharacterStats>();
			if (component.CharacterClass == CharacterStats.Class.Chanter)
			{
				Window.SetActive(value: true);
				Display.Load(component);
			}
			else
			{
				Window.SetActive(value: false);
			}
		}
		else
		{
			Window.SetActive(value: false);
		}
	}
}
