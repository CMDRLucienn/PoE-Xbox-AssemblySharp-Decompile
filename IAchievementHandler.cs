public interface IAchievementHandler
{
	void Update();

	void Initialize();

	void Reinitialize();

	bool IsInitialized();

	bool AchievementsAvailable();

	void UnlockAllAchievements();

	void ResetAchievementUnlocks();

	bool CanAwardAchievement(string achievementIdentifier);

	bool IsAchievementUnlocked(string achievementIdentifier);

	void AwardAchievement(string achievementIdentifier);

	bool AchievementExists(string achievementIdentifier);
}
