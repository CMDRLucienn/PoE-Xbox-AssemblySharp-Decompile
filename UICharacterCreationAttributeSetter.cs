using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UICharacterCreationAttributeSetter : UICharacterCreationElement
{
	public int Adjustment = 1;

	public CharacterStats.AttributeScoreType Stat;

	private void OnClick()
	{
		bool flag = Adjustment > 0;
		for (int i = 0; i < Mathf.Abs(Adjustment); i++)
		{
			if (flag)
			{
				if (IncAllowed())
				{
					base.Owner.Character.BaseStats[(int)Stat]++;
				}
			}
			else if (DecAllowed())
			{
				base.Owner.Character.BaseStats[(int)Stat]--;
			}
		}
		base.Owner.SignalValueChanged(ValueType.Attribute);
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Attribute || type == ValueType.All)
		{
			bool flag = (Adjustment > 0 && IncAllowed()) || (Adjustment < 0 && DecAllowed());
			GetComponent<UIWidget>().enabled = flag;
			GetComponent<UIImageButtonRevised>().enabled = flag;
		}
	}

	private bool IncAllowed()
	{
		return UICharacterCreationManager.Instance.AllowIncStat(base.Owner.Character, Stat);
	}

	private bool DecAllowed()
	{
		return UICharacterCreationManager.Instance.AllowDecStat(base.Owner.Character, Stat);
	}
}
