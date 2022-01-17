using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MemorialContainer : Usable
{
	[Serializable]
	public class Memorial
	{
		public string Name = string.Empty;

		public string Description = string.Empty;

		public Memorial(string name, string desc)
		{
			Name = name;
			Description = desc;
		}
	}

	public int StartingIndex;

	public int NumMemorials;

	public float UseRadius = 3f;

	public float ArrivalDistance;

	[HideInInspector]
	public List<Memorial> m_Memorials = new List<Memorial>();

	private bool m_Loaded;

	public override float UsableRadius => UseRadius;

	public override float ArrivalRadius => ArrivalDistance;

	public override bool IsUsable => base.IsVisible;

	public override bool Use(GameObject user)
	{
		if (!m_Loaded)
		{
			return false;
		}
		UIMemorialManager.Instance.SourceContainer = this;
		UIMemorialManager.Instance.ShowWindow();
		return true;
	}

	private void PopulateMemorials()
	{
		XmlDocument xmlDocument = new XmlDocument();
		string xml = Resources.Load("Data/UI/BackerMemorials").ToString();
		xmlDocument.LoadXml(xml);
		int num = 0;
		string[] array = new string[2];
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (num >= StartingIndex)
			{
				int num2 = 0;
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					array[num2] = childNode2.InnerText;
					num2++;
				}
				m_Memorials.Add(new Memorial(array[0], array[1]));
			}
			num++;
			if (num >= StartingIndex + NumMemorials)
			{
				break;
			}
		}
		m_Loaded = true;
	}

	protected override void Start()
	{
		base.Start();
		PopulateMemorials();
		m_Loaded = true;
	}

	protected override void OnDestroy()
	{
		if (!Application.isEditor || Application.isPlaying)
		{
			m_Memorials.Clear();
			m_Loaded = false;
			base.OnDestroy();
			ComponentUtils.NullOutObjectReferences(this);
		}
	}
}
