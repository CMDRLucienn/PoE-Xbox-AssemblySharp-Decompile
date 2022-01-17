using System.Collections.Generic;
using UnityEngine;

public class NotesManager : MonoBehaviour
{
	[Persistent]
	private List<NotesPage> m_Notes = new List<NotesPage>();

	public static NotesManager Instance { get; private set; }

	public IEnumerable<NotesPage> Notes => m_Notes;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'NotesManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public NotesPage NewNote()
	{
		NotesPage notesPage = new NotesPage();
		notesPage.AddNew();
		m_Notes.Add(notesPage);
		return notesPage;
	}

	public void DeleteNote(NotesPage note)
	{
		m_Notes.Remove(note);
	}
}
