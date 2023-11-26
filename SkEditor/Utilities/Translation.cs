using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using SkEditor.API;
using System;
using System.IO;
using System.Linq;

namespace SkEditor.Utilities;
public class Translation
{
	public static string LanguagesFolder { get; } = $"{AppContext.BaseDirectory}/Languages";

	public static string Get(string key, params string[] parameters)
	{
		Application.Current.TryGetResource(key, Avalonia.Styling.ThemeVariant.Default, out object translation);
		string translationString = translation?.ToString() ?? "?-?";
		translationString = translationString.Replace("{n}", Environment.NewLine);

		for (int i = 0; i < parameters.Length; i++)
		{
			translationString = translationString.Replace($"{{{i}}}", parameters[i]);
		}

		return translationString;
	}

	public static void LoadDefaultLanguage()
	{
		Uri languageXaml = new(Path.Combine(LanguagesFolder, $"English.xaml"));
		using Stream languageStream = File.OpenRead(languageXaml.OriginalString);
		if (AvaloniaRuntimeXamlLoader.Load(languageStream) is ResourceDictionary dictionary)
		{
			Application.Current.Resources.MergedDictionaries.Add(dictionary);
		}
	}

	public async static void ChangeLanguage(string language)
	{
		var translations = Application.Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("/Languages/") ?? false);

		if (translations != null) Application.Current.Resources.MergedDictionaries.Remove(translations);

		Uri languageXaml = new(Path.Combine(LanguagesFolder, $"{language}.xaml"));

		if (!File.Exists(languageXaml.OriginalString))
		{
			ApiVault.Get().GetAppConfig().Language = "English";
			ApiVault.Get().GetAppConfig().Save();
			LoadDefaultLanguage();
			return;
		}

		using Stream languageStream = File.OpenRead(languageXaml.OriginalString);

		if (AvaloniaRuntimeXamlLoader.Load(languageStream) is ResourceDictionary dictionary)
		{
			Application.Current.Resources.MergedDictionaries.Add(dictionary);
		}
	}
}
