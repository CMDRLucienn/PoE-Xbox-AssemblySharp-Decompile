using System.Xml.Linq;

public class BannerModel
{
	public string ImageUrl;

	public string LinkUrl;

	private const string XML_IMAGE = "Image";

	private const string XML_LINK = "Link";

	public BannerModel(XElement parentXML)
	{
		if (parentXML != null)
		{
			XElement xElement = parentXML.Element("Image");
			if (xElement != null)
			{
				ImageUrl = xElement.Value;
			}
			xElement = parentXML.Element("Link");
			if (xElement != null)
			{
				LinkUrl = xElement.Value;
			}
		}
	}
}
