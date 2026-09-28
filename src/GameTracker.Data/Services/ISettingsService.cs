using GameTracker.Core.Settings;

namespace GameTracker.Data.Services
{
    public interface ISettingsService
    {
        Task<T> GetAsync<T>(SettingDefinition<T> definition);

        Task SetAsync<T>(SettingDefinition<T> definition, T value);
    }
}
