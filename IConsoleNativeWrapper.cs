public interface IConsoleNativeWrapper
{
	bool IsSigningIn();

	void Update();

	void Initialize();

	void Reset();

	bool IsInitialized();

	ConsoleUser GetPrimaryUser();

	ConsoleUser GetLastPrimaryUser();

	bool HasPrimaryUser();

	bool HasLastPrimaryUser();

	bool RequestSignInAppUser();

	bool RequestSignIn();

	bool ChangeProfile();

	IAchievementHandler GetAchievementHandler();
}
