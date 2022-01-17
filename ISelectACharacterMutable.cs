public interface ISelectACharacterMutable : ISelectACharacter
{
	new CharacterStats SelectedCharacter { get; set; }
}
