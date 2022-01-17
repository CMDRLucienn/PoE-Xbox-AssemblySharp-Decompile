using System.Collections.Generic;
using System.Linq;
using System.Text;
using Onyx;

public class UIJournalBioText : UIPopulator
{
	public enum Mode
	{
		Biography,
		Visions,
		None
	}

	public class BiographyCategory : ITreeListContent
	{
		public Mode mode;

		public BiographyCategory(Mode type)
		{
			mode = type;
		}

		public string GetTreeListDisplayName()
		{
			return StringTableManager.GetText(DatabaseString.StringTableType.Backstory, (mode == Mode.Biography) ? 402 : 403);
		}
	}

	public void Load(Mode mode)
	{
		Populate(0);
		if ((bool)GameState.s_playerCharacter)
		{
			switch (mode)
			{
			case Mode.Biography:
				LoadBiography();
				break;
			case Mode.Visions:
				LoadPastStory();
				break;
			case Mode.None:
				LoadEmpty();
				break;
			}
		}
	}

	public void LoadBiography()
	{
		int num = 0;
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		ActivateClone(num++).GetComponent<UILabel>().text = JournalBiographyManager.Instance.BiographyData.GetBiographyString(component.CharacterBackground);
		if (JournalBiographyManager.Instance.PresentStoryUnlockedTimes == null)
		{
			return;
		}
		JournalStoryData.StoryItem[] items = JournalBiographyManager.Instance.PresentStoryData.Items;
		List<int> list = new List<int>();
		for (int j = 0; j < items.Length; j++)
		{
			if (JournalBiographyManager.Instance.IsPresentUnlocked(j))
			{
				list.Add(j);
			}
		}
		IOrderedEnumerable<int> orderedEnumerable = list.OrderBy((int i) => JournalBiographyManager.Instance.PresentStoryUnlockedTimes[i]);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int item in orderedEnumerable)
		{
			string text = items[item].Text.GetText();
			if (StringExtender.IsNullOrWhitespace(text))
			{
				if (!StringBuilderExtender.IsNullOrWhitespace(stringBuilder))
				{
					ActivateClone(num++).GetComponent<UILabel>().text = Conversation.ReplacePlayerTokens(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
			}
			else
			{
				stringBuilder.Append(text.Trim());
				stringBuilder.Append(' ');
			}
		}
		if (!StringBuilderExtender.IsNullOrWhitespace(stringBuilder))
		{
			ActivateClone(num++).GetComponent<UILabel>().text = Conversation.ReplacePlayerTokens(stringBuilder.ToString());
		}
	}

	public void LoadPastStory()
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < JournalBiographyManager.Instance.PastStoryData.Items.Length; i++)
		{
			if (!JournalBiographyManager.Instance.IsPastUnlocked(i))
			{
				continue;
			}
			string text = JournalBiographyManager.Instance.PastStoryData.Items[i].Text.GetText();
			if (StringExtender.IsNullOrWhitespace(text))
			{
				if (!StringBuilderExtender.IsNullOrWhitespace(stringBuilder))
				{
					ActivateClone(num++).GetComponent<UILabel>().text = Conversation.ReplacePlayerTokens(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
			}
			else
			{
				stringBuilder.Append(text.Trim());
				stringBuilder.Append(' ');
			}
		}
		if (!StringBuilderExtender.IsNullOrWhitespace(stringBuilder))
		{
			ActivateClone(num++).GetComponent<UILabel>().text = Conversation.ReplacePlayerTokens(stringBuilder.ToString());
		}
	}

	public void LoadEmpty()
	{
		ActivateClone(0).GetComponent<UILabel>().text = "";
	}
}
