using UnityEngine;

public class UIStrongholdBreakdownLine : MonoBehaviour
{
	public UILabel ValueLabel;

	public UILabel CauseLabel;

	public void Set(int value, string cause)
	{
		ValueLabel.text = TextUtils.NumberBonus(value);
		CauseLabel.text = cause;
	}
}
