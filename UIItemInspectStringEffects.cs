using System.Collections.Generic;
using UnityEngine;

public class UIItemInspectStringEffects : UIPopulator
{
	public void Load(StringEffects stringEffects)
	{
		Populate(0);
		int num = 0;
		foreach (KeyValuePair<string, List<AttackBase.AttackEffect>> effect in stringEffects.Effects)
		{
			GameObject gameObject = ActivateClone(num++);
			UIItemInspectStringEffect component = gameObject.GetComponent<UIItemInspectStringEffect>();
			component.Load(effect);
			for (int i = 0; i < m_Clones.Count; i++)
			{
				if (m_Clones[i].activeSelf && m_Clones[i] != gameObject)
				{
					UIItemInspectStringEffect component2 = m_Clones[i].GetComponent<UIItemInspectStringEffect>();
					if ((bool)component2 && component2.EffectsLabel.text == component.EffectsLabel.text)
					{
						component.EffectsLabel.text = GUIUtils.Format(2316, component2.TargetName);
					}
				}
			}
		}
	}

	public void Clear()
	{
		Populate(0);
	}
}
