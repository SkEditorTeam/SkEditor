using System.ComponentModel;
using R3;
using SkEditor.Utilities.Settings;

namespace SkEditor.Utilities.Extensions;

public static class SettingsExtensions
{
    extension<T>(BaseConfig<T> config) where T : BaseConfig<T>, IConfig, new()
    {
        public Observable<PropertyChangedEventArgs> ObserveAnyPropertyChanged()
        {
            return Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => (_, e) => h(e),
                h => config.PropertyChanged += h,
                h => config.PropertyChanged -= h
            );
        }
    }
}