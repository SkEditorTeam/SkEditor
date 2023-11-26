using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace SkEditor.Utilities.Syntax;
public class SyntaxLoader
{
	private static string SyntaxFolder { get; set; } = Path.Combine(AppConfig.AppDataFolderPath, "Syntax Highlighting");

	public static HashSet<string> Syntaxes { get; set; } = [];

	// File, extensions
	public static Dictionary<string, List<string>> OtherLanguageSyntaxes { get; set; } = [];

	public static string SyntaxFilePath => Path.Combine(SyntaxFolder, ApiVault.Get().GetAppConfig().CurrentSyntax);

	public async static void LoadSyntaxes()
	{
		Directory.CreateDirectory(SyntaxFolder);
		Directory.CreateDirectory(Path.Combine(SyntaxFolder, "Other Languages"));

		Directory.GetFiles(SyntaxFolder).Where(file => Path.GetExtension(file).Equals(".xshd")).ToList().ForEach(file =>
		{
			Syntaxes.Add(Path.GetFileName(file));
		});

		Directory.GetFiles(Path.Combine(SyntaxFolder, "Other Languages")).Where(file => Path.GetExtension(file).Equals(".xshd")).ToList().ForEach(file =>
		{
			string[] extensions = Path.GetFileNameWithoutExtension(file).Split('-');
			OtherLanguageSyntaxes.Add(Path.GetFileName(file), extensions.ToList());
		});

		string currentSyntax = ApiVault.Get().GetAppConfig().CurrentSyntax;

		if (!Syntaxes.Contains(currentSyntax))
		{
			ApiVault.Get().GetAppConfig().CurrentSyntax = "Default.xshd";
			if (!File.Exists(Path.Combine(SyntaxFolder, "Default.xshd")))
			{
				await DownloadSyntax();
			}
		}

		UpdateSyntax(SyntaxFilePath);
	}

	public static async Task DownloadSyntax()
	{
		try
		{
			HttpClient client = new();
			string url = "https://marketplace-skeditor.vercel.app/SkEditorFiles/Default.xshd";
			using var stream = await client.GetStreamAsync(url);
			using var fileStream = File.Create(Path.Combine(SyntaxFolder, "Default.xshd"));
			await stream.CopyToAsync(fileStream);

			Syntaxes.Add("Default.xshd");

			UpdateSyntax(Path.Combine(SyntaxFolder, "Default.xshd"));
		}
		catch
		{
			await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Error"), Translation.Get("FailedToDownloadSyntax"), new SymbolIconSource() { Symbol = Symbol.ImportantFilled }, primaryButton: false);
		}
	}

	public static void UpdateSyntax(string syntaxFilePath)
	{
		try
		{
			using var reader = XmlReader.Create(new StreamReader(syntaxFilePath));
			ApiVault.Get().GetTabView().TabItems.Cast<TabViewItem>().ToList().Where(ApiVault.Get().IsFile).ToList().ForEach(tab =>
			{
				TextEditor editor = (TextEditor)tab.Content;
				editor.SyntaxHighlighting = AvaloniaEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
			});
			reader.Close();
		}
		catch { }
	}

	public static void RefreshSyntax()
	{
		try
		{
			using var reader = XmlReader.Create(new StreamReader(SyntaxFilePath));
			ApiVault.Get().GetTextEditor().SyntaxHighlighting = AvaloniaEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
			reader.Close();
		}
		catch
		{
			ApiVault.Get().ShowMessageWithIcon("Error", "Failed to refresh syntax highlighting", new SymbolIconSource() { Symbol = Symbol.ImportantFilled });
		}
	}

	public static void SetSyntax(TextEditor editor, string filePath = "")
	{
		string syntax = SyntaxFilePath;
		if (!string.IsNullOrWhiteSpace(filePath))
		{
			string extension = Path.GetExtension(filePath).TrimStart('.');
			if (OtherLanguageSyntaxes.Any(x => x.Value.Contains(extension)))
			{
				syntax = Path.Combine(SyntaxFolder, "Other Languages", OtherLanguageSyntaxes.First(x => x.Value.Contains(extension)).Key);
			}
		}

		try
		{
			using var reader = XmlReader.Create(new StreamReader(syntax));
			editor.SyntaxHighlighting = AvaloniaEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
			reader.Close();
		}
		catch (Exception e)
		{
			Log.Error($"Failed to set syntax highlighting\n\n{e.Message}\n{e.StackTrace}");
		}
	}
}
