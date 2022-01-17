public interface ISelectACharacter
{
	CharacterStats SelectedCharacter { get; }

	event SelectedCharacterChanged OnSelectedCharacterChanged;
}
