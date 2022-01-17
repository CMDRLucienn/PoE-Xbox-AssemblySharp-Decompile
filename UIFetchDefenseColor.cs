using UnityEngine;

[ExecuteInEditMode]
public class UIFetchDefenseColor : MonoBehaviour
{
	public CharacterStats.DefenseType DefenseType = CharacterStats.DefenseType.None;

	private void Awake()
	{
		UpdateColor();
	}

	private void UpdateColor()
	{
		Color iconColor = UIPartyMemberStatIconGetter.GetIconColor(UIPartyMemberStatIconGetter.GetDefenseTypeSprite(DefenseType));
		UIWidget component = GetComponent<UIWidget>();
		if ((bool)component)
		{
			component.color = iconColor;
		}
		UIImageButtonRevised component2 = GetComponent<UIImageButtonRevised>();
		if ((bool)component2)
		{
			component2.SetNeutralColor(iconColor);
		}
	}
}
