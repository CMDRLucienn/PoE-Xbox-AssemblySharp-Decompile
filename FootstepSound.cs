using System;

[Serializable]
public class FootstepSound
{
	public GroundMaterial SoundMaterial;

	public ClipBankSet Clips = new ClipBankSet();

	public override bool Equals(object obj)
	{
		if (obj is GroundMaterial)
		{
			return (GroundMaterial)obj == SoundMaterial;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return SoundMaterial.GetHashCode();
	}
}
