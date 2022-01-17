using System.Collections.Generic;

public class UICharacterSheetStatsPopulator : UICharacterSheetContentLine
{
	public UICharacterSheetStatLine RootLine;

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

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void Initialize()
	{
		if (m_Done)
		{
			return;
		}
		m_Done = true;
		List<CharacterStats.AttributeScoreType> list = new List<CharacterStats.AttributeScoreType>();
		list.Add(CharacterStats.AttributeScoreType.Might);
		list.Add(CharacterStats.AttributeScoreType.Constitution);
		list.Add(CharacterStats.AttributeScoreType.Dexterity);
		list.Add(CharacterStats.AttributeScoreType.Perception);
		list.Add(CharacterStats.AttributeScoreType.Intellect);
		list.Add(CharacterStats.AttributeScoreType.Resolve);
		for (int i = 0; i < 6; i++)
		{
			UICharacterSheetStatLine uICharacterSheetStatLine = RootLine;
			if (i > 0)
			{
				uICharacterSheetStatLine = NGUITools.AddChild(RootLine.transform.parent.gameObject, RootLine.gameObject).GetComponent<UICharacterSheetStatLine>();
				uICharacterSheetStatLine.transform.localPosition = RootLine.transform.localPosition;
			}
			uICharacterSheetStatLine.Attribute = (CharacterStats.AttributeScoreType)i;
			uICharacterSheetStatLine.gameObject.name = list.IndexOf((CharacterStats.AttributeScoreType)i) + "." + uICharacterSheetStatLine.Attribute;
		}
		Table.Reposition();
	}

	public override void Load(CharacterStats stats)
	{
		Table.repositionNow = true;
	}
}
