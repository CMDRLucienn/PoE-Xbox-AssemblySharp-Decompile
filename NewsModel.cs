using System;
using System.Globalization;
using System.Xml.Linq;

public class NewsModel : IComparable<NewsModel>
{
	public int ID;

	public string Title;

	public string Ticker;

	public DateTime Date;

	public string Description;

	public bool ShouldDisplay;

	private const string XML_ID = "id";

	private const string XML_Date = "Date";

	private const string XML_Title = "Title";

	private const string XML_Ticker = "Ticker";

	private const string XML_Description = "Content";

	private const string XML_Requirements = "Requirements";

	private const string XML_MinBuild = "MinBuild";

	private const string XML_MaxBuild = "MaxBuild";

	public NewsModel(XElement parentXML)
	{
		if (parentXML != null)
		{
			XAttribute xAttribute = parentXML.Attribute("id");
			if (xAttribute != null)
			{
				int.TryParse(xAttribute.Value, out ID);
			}
			XElement xElement = parentXML.Element("Title");
			if (xElement != null)
			{
				Title = xElement.Value;
			}
			xElement = parentXML.Element("Ticker");
			if (xElement != null)
			{
				Ticker = xElement.Value;
			}
			xElement = parentXML.Element("Date");
			if (xElement != null)
			{
				DateTime.TryParse(xElement.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out Date);
			}
			xElement = parentXML.Element("Content");
			if (xElement != null)
			{
				Description = xElement.Value;
			}
			xElement = parentXML.Element("Requirements");
			DetermineVisibility(xElement);
		}
	}

	public int CompareTo(NewsModel other)
	{
		return other.Date.CompareTo(Date);
	}

	private void DetermineVisibility(XElement parentRequirements)
	{
		if (parentRequirements == null)
		{
			ShouldDisplay = true;
			return;
		}
		int bUILD_NUMBER = buildnum.BUILD_NUMBER;
		int result = -1;
		XElement xElement = parentRequirements.Element("MinBuild");
		if (xElement != null && int.TryParse(xElement.Value, out result) && bUILD_NUMBER < result)
		{
			ShouldDisplay = false;
			return;
		}
		int result2 = -1;
		xElement = parentRequirements.Element("MaxBuild");
		if (xElement != null && int.TryParse(xElement.Value, out result2) && bUILD_NUMBER > result2)
		{
			ShouldDisplay = false;
		}
		else
		{
			ShouldDisplay = true;
		}
	}
}
