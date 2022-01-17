using System;

public static class ProductConfiguration
{
	[Flags]
	public enum Package
	{
		BaseGame = 1,
		BackerBeta = 2,
		Expansion1 = 4,
		Expansion2 = 8,
		Expansion4 = 0x10
	}

	public static readonly int MajorVersion = 1;

	public static readonly int MinorVersion = 3;

	public static readonly int PatchVersion = 4;

	public static readonly Package CurrentPackage = Package.Expansion4;

	public static Package ActivePackage = Package.BaseGame;

	public static readonly string[] PackageDataFolders = new string[5] { "data", "data_backerbeta", "data_expansion1", "data_expansion2", "data_expansion4" };

	public static int Version => MajorVersion * 100 + MinorVersion;

	public static string GetVersion()
	{
		return StringUtility.Format("v{0}.{1}.{2}.{3}", MajorVersion, MinorVersion, PatchVersion, buildnum.BUILD_NUMBER.ToString("0000"));
	}
}
