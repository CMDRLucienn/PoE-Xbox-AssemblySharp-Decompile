public class UICharacterSheetSkillsPopulator : UICharacterSheetContentLine
{
	public UICharacterSheetSkillLine RootLine;

	public UITable Table;

	private bool m_Done;

	private bool m_NeedsRepo;

	private void OnEnable()
	{
		m_NeedsRepo = true;
	}

	private void Update()
	{
		if (m_NeedsRepo)
		{
			m_NeedsRepo = false;
			Table.Reposition();
		}
	}

	public override void Initialize()
	{
		if (m_Done)
		{
			return;
		}
		m_Done = true;
		for (int i = 0; i < 6; i++)
		{
			if (i != 5)
			{
				UICharacterSheetSkillLine uICharacterSheetSkillLine = RootLine;
				if (i > 0)
				{
					uICharacterSheetSkillLine = NGUITools.AddChild(RootLine.transform.parent.gameObject, RootLine.gameObject).GetComponent<UICharacterSheetSkillLine>();
					uICharacterSheetSkillLine.transform.localPosition = RootLine.transform.localPosition;
				}
				uICharacterSheetSkillLine.Skill = (CharacterStats.SkillType)i;
				uICharacterSheetSkillLine.gameObject.name = i + "." + uICharacterSheetSkillLine.Skill.ToString();
			}
		}
		Table.Reposition();
	}

	public override void Load(CharacterStats stats)
	{
		Table.repositionNow = true;
	}
}
