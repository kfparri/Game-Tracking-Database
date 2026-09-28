namespace GameTracker.Core.Settings
{
    public static class AppSettingsKeys
    {
        public static readonly SettingDefinition<DateTime?> LastBackupTime =
            new("application.settings.lastbackuptime", null);
    }
}
