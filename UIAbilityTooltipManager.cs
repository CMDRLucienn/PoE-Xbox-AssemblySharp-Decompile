using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIAbilityTooltipManager : UIPopulator
{
	public static UIAbilityTooltipManager Instance { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
		GameState.OnLevelUnload += OnLoadedSave;
	}

	private void OnLoadedSave(object sender, EventArgs e)
	{
		HideAll();
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelUnload -= OnLoadedSave;
		base.OnDestroy();
	}

	public UIWidget GetBg(int index)
	{
		if (m_Clones.Count > index)
		{
			return m_Clones[index].GetComponent<UIAbilityTooltip>().Background;
		}
		return null;
	}

	public void Show(int index, UIWidget element, IEnumerable<ITooltipContent> data)
	{
		Show(index, element, null, UIWidget.Pivot.BottomLeft, data);
	}

	public void Show(int index, UIWidget element, GameObject owner, IEnumerable<ITooltipContent> data)
	{
		Show(index, element, owner, UIWidget.Pivot.BottomLeft, data);
	}

	public void Show(int index, UIWidget element, GameObject owner, UIWidget.Pivot pivot, IEnumerable<ITooltipContent> data)
	{
		Show(index, element, owner, pivot, data, Vector2.zero);
	}

	public void Show(int index, UIWidget element, GameObject owner, UIWidget.Pivot pivot, IEnumerable<ITooltipContent> data, Vector2 offset)
	{
		if (data.Any())
		{
			UIAbilityTooltip component = ActivateClone(index).GetComponent<UIAbilityTooltip>();
			component.Show(element, data, pivot, owner);
			UIAnchor component2 = component.GetComponent<UIAnchor>();
			if (component2 != null)
			{
				component2.pixelOffset = offset;
			}
		}
	}

	public void Show(int index, UIWidget element, params ITooltipContent[] data)
	{
		Show(index, element, null, UIWidget.Pivot.BottomLeft, data);
	}

	public void Show(int index, UIWidget element, GameObject owner, params ITooltipContent[] data)
	{
		Show(index, element, owner, UIWidget.Pivot.BottomLeft, data);
	}

	public void Show(int index, UIWidget element, GameObject owner, UIWidget.Pivot prefer, params ITooltipContent[] data)
	{
		ActivateClone(index).GetComponent<UIAbilityTooltip>().Show(element, data, prefer, owner);
	}

	public void Show(int index, UIWidget element, int iconSize, params ITooltipContent[] data)
	{
		ActivateClone(index).GetComponent<UIAbilityTooltip>().Show(element, data, UIWidget.Pivot.BottomLeft, iconSize);
	}

	public void Show(int index, Vector3 position, GameObject owner, UIWidget.Pivot prefer, params ITooltipContent[] data)
	{
		ActivateClone(index).GetComponent<UIAbilityTooltip>().Show(position, data, prefer, owner);
	}

	public void Hide(int index)
	{
		ActivateClone(index).GetComponent<UIAbilityTooltip>().Hide();
	}

	public void HideAll()
	{
		if (m_Clones == null)
		{
			return;
		}
		foreach (GameObject clone in m_Clones)
		{
			if ((bool)clone)
			{
				clone.GetComponent<UIAbilityTooltip>().Hide();
			}
		}
	}
}
