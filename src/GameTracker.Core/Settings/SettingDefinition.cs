namespace GameTracker.Core.Settings
{
    public class SettingDefinition<T>
    {
        public string Key { get; }
        public T DefaultValue { get; }

        public SettingDefinition(string key, T defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
        }
    }
}
