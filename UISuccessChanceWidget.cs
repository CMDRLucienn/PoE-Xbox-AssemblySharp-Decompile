using UnityEngine;

public class UISuccessChanceWidget : MonoBehaviour
{
	public UISprite DefenseType;

	public UILabel Label;

	public UISprite GlowSprite;

	private UIAnchorToWorld m_WorldAnchor;

	private CharacterStats m_Attacker;

	private CharacterStats m_Defender;

	private AttackBase m_Attack;

	private GenericAbility m_Ability;

	[HideInInspector]
	public bool KeepAlive;

	private void Awake()
	{
		m_WorldAnchor = GetComponent<UIAnchorToWorld>();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		UpdateValue();
		m_WorldAnchor.UpdatePosition();
		base.transform.localPosition = m_WorldAnchor.Position;
		float num = (((bool)m_WorldAnchor.AnchorFaction && m_WorldAnchor.AnchorFaction.isFowVisible) ? 1f : 0f);
		Label.alpha = num;
		GlowSprite.alpha = num * UISuccessChanceManager.Instance.Alpha;
		DefenseType.alpha = num;
	}

	public void DoKeepAlive(CharacterStats attacker, AttackBase attack, GenericAbility ability)
	{
		m_Attacker = attacker;
		m_Attack = attack;
		m_Ability = ability;
		KeepAlive = true;
	}

	public void Set(CharacterStats attacker, CharacterStats defender, AttackBase attack, GenericAbility ability)
	{
		m_Attacker = attacker;
		m_Defender = defender;
		m_Attack = attack;
		m_Ability = ability;
		KeepAlive = true;
		base.gameObject.SetActive(value: true);
		m_WorldAnchor.SetAnchor(defender.gameObject);
		Update();
	}

	public void ApplyColors(UISuccessChanceManager.ColorPair colors)
	{
		Label.color = colors.Text;
		DefenseType.color = Label.color;
		GlowSprite.color = colors.Background;
	}

	private void UpdateValue()
	{
		if (m_Attacker == null || m_Defender == null)
		{
			KeepAlive = false;
			return;
		}
		BestiaryCertainty known;
		int perceivedDefense = m_Defender.GetPerceivedDefense(m_Attack.DefendedBy, out known);
		DefenseType.spriteName = UIPartyMemberStatIconGetter.GetDefenseTypeSprite(m_Attack.DefendedBy);
		if (known == BestiaryCertainty.Unknown)
		{
			Label.text = GUIUtils.Format(1277, GUIUtils.GetText(1980));
			ApplyColors(UISuccessChanceManager.Instance.UnknownColors);
			return;
		}
		int num = m_Attacker.CalculateAccuracyForUi(m_Attack, m_Ability, m_Defender.gameObject);
		int num2 = m_Defender.CalculateDefense((m_Attack.DefendedBy != CharacterStats.DefenseType.None) ? m_Attack.DefendedBy : m_Attack.SecondaryDefense);
		string text = "";
		if (known == BestiaryCertainty.Estimated)
		{
			num2 = perceivedDefense;
			text = GUIUtils.GetText(1980);
		}
		int num3 = num - num2;
		float num4 = CharacterStats.GrazeThreshhold - (float)num3;
		float num5 = Mathf.Clamp01(1f - num4 / 100f);
		Label.text = GUIUtils.Format(1277, (num5 * 100f).ToString("#0")) + text;
		ApplyColors(UISuccessChanceManager.Instance.GetColorsForRatio(num5));
	}
}
