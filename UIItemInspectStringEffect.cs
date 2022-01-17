using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIItemInspectStringEffect : MonoBehaviour
{
	public UILabel TargetNameLabel;

	public UILabel EffectsLabel;

	public string TargetName { get; private set; }

	public void Load(KeyValuePair<string, List<AttackBase.AttackEffect>> kv)
	{
		IEnumerable<IGrouping<AttackBase.AttackEffect, AttackBase.AttackEffect>> enumerable = from ae in kv.Value
			group ae by ae;
		TargetName = kv.Key;
		TargetNameLabel.text = TargetName + ":";
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IGrouping<AttackBase.AttackEffect, AttackBase.AttackEffect> item in enumerable)
		{
			if (!item.Any())
			{
				continue;
			}
			AttackBase.AttackEffect key = item.Key;
			StringBuilder stringBuilder2 = new StringBuilder();
			AttackBase attack = item.First().Attack;
			AttackRanged attackRanged = attack as AttackRanged;
			bool flag = !item.First().ConsiderSecondary;
			bool flag2 = item.Any((AttackBase.AttackEffect effect) => effect.Hostile);
			string text = TextUtils.FuncJoin((AttackBase.AttackEffect ae) => ae.ToString(), item, GUIUtils.Comma());
			CharacterStats.DefenseType defenseType = CharacterStats.DefenseType.None;
			if (key.OverrideDefenseType != CharacterStats.DefenseType.None)
			{
				defenseType = key.OverrideDefenseType;
			}
			else if (flag2 && (bool)attack && (flag ? attack.DefendedBy : attack.SecondaryDefense) != CharacterStats.DefenseType.None)
			{
				defenseType = (flag ? attack.DefendedBy : attack.SecondaryDefense);
			}
			if (defenseType != CharacterStats.DefenseType.None)
			{
				string text2 = GUIUtils.GetText(369);
				if (flag2 && (bool)attack && attack.AccuracyBonusTotal != 0)
				{
					text2 += GUIUtils.Format(1731, TextUtils.NumberBonus(attack.AccuracyBonusTotal));
				}
				if ((bool)attackRanged && attackRanged.VeilPiercing)
				{
					text2 += GUIUtils.Format(1731, GUIUtils.GetText(2327));
				}
				text = text + " | " + GUIUtils.Format(1605, text2, GUIUtils.GetDefenseTypeString(defenseType));
			}
			if (item.First().EffectPostFormat != "{0}")
			{
				text = StringUtility.Format(item.First().EffectPostFormat, text);
			}
			stringBuilder2.Append(text);
			string text3 = stringBuilder2.ToString();
			if (key.Attack is AttackBeam)
			{
				AttackBeam attackBeam = key.Attack as AttackBeam;
				text3 = GUIUtils.Format(1419, text3, GUIUtils.Format(211, attackBeam.BeamInterval.ToString("#0.0")));
			}
			if (!flag && defenseType != CharacterStats.DefenseType.None && attack.DefendedBy != CharacterStats.DefenseType.None)
			{
				text3 = GUIUtils.Format(2315, text3);
			}
			stringBuilder.AppendLine();
			stringBuilder.Append(text3);
		}
		EffectsLabel.text = Glossary.Instance.AddUrlTags(stringBuilder.ToString().Trim());
	}
}
