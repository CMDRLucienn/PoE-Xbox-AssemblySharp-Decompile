using UnityEngine;

public class UICharacterCreationSkillSetter : UICharacterCreationElement
{
	public int Adjustment = 1;

	public CharacterStats.SkillType Skill;

	private void Awake()
	{
		bool flag = (Adjustment > 0 && IncAllowed()) || (Adjustment < 0 && DecAllowed());
		GetComponent<UIWidget>().enabled = flag;
		GetComponent<UIImageButtonRevised>().enabled = flag;
	}

	private void OnClick()
	{
		bool flag = Adjustment > 0;
		for (int i = 0; i < Mathf.Abs(Adjustment); i++)
		{
			if (flag)
			{
				if (IncAllowed())
				{
					int pointsNeededForIncrement = GetPointsNeededForIncrement();
					base.Owner.Character.SkillValueDeltas[(int)Skill] += pointsNeededForIncrement;
					base.Owner.Character.SkillPointsToSpend -= pointsNeededForIncrement;
					base.Owner.Character.SkillRankDeltas[(int)Skill]++;
				}
			}
			else if (DecAllowed())
			{
				int pointsRecievedOnDecrement = GetPointsRecievedOnDecrement();
				base.Owner.Character.SkillValueDeltas[(int)Skill] -= pointsRecievedOnDecrement;
				base.Owner.Character.SkillPointsToSpend += pointsRecievedOnDecrement;
				base.Owner.Character.SkillRankDeltas[(int)Skill]--;
			}
		}
		base.Owner.SignalValueChanged(ValueType.Skill);
	}

	public override void SignalValueChanged(ValueType type)
	{
		bool flag = (Adjustment > 0 && IncAllowed()) || (Adjustment < 0 && DecAllowed());
		GetComponent<UIWidget>().enabled = flag;
		GetComponent<UIImageButtonRevised>().enabled = flag;
	}

	private int GetPointsNeededForIncrement()
	{
		int num = base.Owner.Character.SkillValues[(int)Skill] + base.Owner.Character.SkillValueDeltas[(int)Skill];
		return CharacterStats.GetPointsForSkillLevel(CharacterStats.CalculateSkillLevelViaPoints(num) + 1) - num;
	}

	private int GetPointsRecievedOnDecrement()
	{
		int num = base.Owner.Character.SkillValues[(int)Skill] + base.Owner.Character.SkillValueDeltas[(int)Skill];
		return num - CharacterStats.GetPointsForSkillLevel(CharacterStats.CalculateSkillLevelViaPoints(num) - 1);
	}

	private bool IncAllowed()
	{
		return GetPointsNeededForIncrement() <= base.Owner.Character.SkillPointsToSpend;
	}

	private bool DecAllowed()
	{
		return base.Owner.Character.SkillValueDeltas[(int)Skill] > 0;
	}
}
