using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class NotesPage : ITreeListContent
{
	[Serializable]
	public class NoteEntry
	{
		public string Text;

		public EternityDateTime Timestamp { get; set; }

		public string SerializedText
		{
			get
			{
				return Text;
			}
			set
			{
				Text = value;
			}
		}

		public void TimestampNow()
		{
			Timestamp = new EternityDateTime(WorldTime.Instance.CurrentTime);
		}

		public void Backspace()
		{
			if (Text.Length > 0)
			{
				Text = Text.Substring(0, Text.Length - 1);
			}
		}
	}

	public List<NoteEntry> Notes = new List<NoteEntry>();

	public DatabaseString LocalizedTitle { get; set; }

	public string UserTitle { get; set; }

	public string SerializedTitle
	{
		get
		{
			return UserTitle;
		}
		set
		{
			UserTitle = value;
		}
	}

	public string DisplayTitle
	{
		get
		{
			if (LocalizedTitle != null)
			{
				return LocalizedTitle.GetText();
			}
			return UserTitle;
		}
	}

	public List<NoteEntry> SerializedNotes
	{
		get
		{
			return Notes;
		}
		set
		{
			Notes = value;
		}
	}

	public string GetFullText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < Notes.Count; i++)
		{
			stringBuilder.AppendLine(Notes[i].Text);
		}
		return stringBuilder.ToString();
	}

	public void SetFullText(string text)
	{
		string[] array = text.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (i >= Notes.Count)
			{
				AddNew();
			}
			Notes[i].Text = array[i];
		}
		for (int num = Notes.Count - 1; num >= array.Length; num--)
		{
			Notes.RemoveAt(num);
		}
	}

	public NotesPage()
	{
	}

	public NotesPage(DatabaseString dbstring)
		: this()
	{
		LocalizedTitle = dbstring;
		UserTitle = null;
	}

	public NotesPage(string title)
		: this()
	{
		UserTitle = title;
		LocalizedTitle = null;
	}

	public void AddNew()
	{
		Notes.Add(new NoteEntry());
		Notes[Notes.Count - 1].TimestampNow();
	}

	public void RemoveLast()
	{
		Notes.RemoveAt(Notes.Count - 1);
	}

	public string GetTreeListDisplayName()
	{
		string text = DisplayTitle;
		if (string.IsNullOrEmpty(text))
		{
			text = GUIUtils.GetText(171);
		}
		return text;
	}
}
