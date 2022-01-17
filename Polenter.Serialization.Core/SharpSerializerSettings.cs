namespace Polenter.Serialization.Core;

public abstract class SharpSerializerSettings<T> where T : AdvancedSharpSerializerSettings, new()
{
	private T _advancedSettings;

	public T AdvancedSettings
	{
		get
		{
			if (_advancedSettings == null)
			{
				_advancedSettings = new T();
			}
			return _advancedSettings;
		}
		set
		{
			_advancedSettings = value;
		}
	}

	public bool IncludeAssemblyVersionInTypeName { get; set; }

	public bool IncludeCultureInTypeName { get; set; }

	public bool IncludePublicKeyTokenInTypeName { get; set; }

	protected SharpSerializerSettings()
	{
		IncludeAssemblyVersionInTypeName = true;
		IncludeCultureInTypeName = true;
		IncludePublicKeyTokenInTypeName = true;
	}
}
