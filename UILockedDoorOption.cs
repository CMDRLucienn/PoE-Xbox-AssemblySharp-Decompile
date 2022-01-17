using UnityEngine;

public class UILockedDoorOption : MonoBehaviour
{
	public UILabel MechanicsLabel;

	public UILabel LockpicksLabel;

	public int MechanicsOffset;

	public void Load(CharacterStats character, OCL container)
	{
		int num = (character ? character.CalculateSkill(CharacterStats.SkillType.Mechanics) : 0);
		int num2 = (container ? (container.LockDifficulty + MechanicsOffset) : 0);
		MechanicsLabel.text = num2.ToString();
		MechanicsLabel.color = ((num < num2) ? Color.red : Color.white);
		int num3 = (container ? PartyHelper.PartyItemCount(container.LockPickItem) : 0);
		int num4 = (container ? container.RequiredLockpicks(num2) : 0);
		LockpicksLabel.text = num4.ToString();
		LockpicksLabel.color = ((num3 < num4) ? Color.red : Color.white);
	}
}
