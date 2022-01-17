using System;
using UnityEngine;

public class GenericCipherAbility : GenericAbility
{
	public float DurationAfterBreak;

	public float FocusCost;

	public bool TicksAfterBreak;

	public float IncreasePerTick;

	public int SpellLevel = 1;

	protected override void CalculateWhyNotReady()
	{
		base.CalculateWhyNotReady();
		if ((bool)Owner)
		{
			CharacterStats component = Owner.GetComponent<CharacterStats>();
			if (component != null && component.Focus < FocusCost)
			{
				base.WhyNotReady = NotReadyValue.NotEnoughFocus;
			}
		}
	}

	public override void RemoveConsumables()
	{
		base.RemoveConsumables();
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component != null)
		{
			float focus = component.Focus;
			focus = (component.Focus = focus - FocusCost);
		}
	}

	public override void HandleGameUtilitiesOnCombatEnd(object sender, EventArgs e)
	{
		base.HandleGameUtilitiesOnCombatEnd(sender, e);
		if (Activated && !Modal && !Passive)
		{
			Debug.LogError("Deactivating cipher ability " + Name() + " because combat has ended and it is still activated.");
			Deactivate(null);
		}
	}

	protected override string GetResourceString()
	{
		string text = GUIUtils.Format(76, FocusCost, GUIUtils.GetText(415));
		string resourceString = base.GetResourceString();
		if (!string.IsNullOrEmpty(resourceString))
		{
			return text + "\n" + resourceString;
		}
		return text;
	}
}
