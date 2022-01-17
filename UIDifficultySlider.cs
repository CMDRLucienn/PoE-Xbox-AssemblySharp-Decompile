using System;
using UnityEngine;

public class UIDifficultySlider : UIOptionsSlider
{
	public UIOptionsTag[] Settings;

	protected bool InGame => UIOptionsManager.Instance.NormalSubWindow.activeSelf;

	public override int Setting
	{
		set
		{
			if (base.enabled)
			{
				if (InGame && GameState.Mode.Difficulty != GameDifficulty.PathOfTheDamned)
				{
					value = Mathf.Clamp(value, 0, 2);
				}
				base.Setting = value;
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnEnable()
	{
		if (InGame && GameState.Mode.Difficulty == GameDifficulty.PathOfTheDamned)
		{
			Setting = 3;
			base.enabled = false;
		}
		else
		{
			base.enabled = true;
		}
		for (int i = 0; i < Settings.Length; i++)
		{
			if (InGame)
			{
				if (i == 3)
				{
					Settings[i].Enable(GameState.Mode.Difficulty == GameDifficulty.PathOfTheDamned);
				}
				else
				{
					Settings[i].Enable(GameState.Mode.Difficulty != GameDifficulty.PathOfTheDamned);
				}
			}
			else
			{
				Settings[i].Enable();
			}
		}
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		OnEnable();
	}
}
