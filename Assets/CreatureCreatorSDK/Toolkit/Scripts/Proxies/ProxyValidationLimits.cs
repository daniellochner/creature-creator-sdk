namespace DanielLochner.CreatureCrafter.SDK
{
    public static class ProxyValidationLimits
    {
        public const int MaxCreatureDisplays = 5;
        public const int MaxSpawners = 10;
        public const int MaxCustomObjects = 15;
        public const int MaxNpcSpawners = 10;
        public const int MaxBattles = 5;
        public const int MaxMinigames = 5;
        public const int MaxQuests = 10;
        public const int MaxTeleports = 5;

        public const int MaxPlayers = 12;

        public const int MaxRoundsPerBattle = MaxNpcSpawners;
        public const int MaxQuestItemsPerQuest = MaxCustomObjects;
        public const int MaxMarkersPerProxy = MaxCustomObjects;
    }
}
