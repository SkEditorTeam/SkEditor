using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;
public class ChangeChecker
{
	private static Dictionary<TextEditor, string> LastKnownContentDictionary = [];
	private static string GetLastKnownContent(TextEditor textEditor) =>
		LastKnownContentDictionary.TryGetValue(textEditor, out string lastKnownContent) ? lastKnownContent : "";
	private static void SetLastKnownContent(TextEditor textEditor, string content) => LastKnownContentDictionary[textEditor] = content;

	private static bool IsMessageShown = false;

	public async static void Check()
	{
		if (!ApiVault.Get().IsFileOpen()) return;

		TabViewItem item = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
		string path = item.Tag.ToString();
		if (string.IsNullOrEmpty(path)) return;
		if (!File.Exists(path)) return;

		try
		{
			string textToWrite = ApiVault.Get().GetTextEditor().Document.Text;
			using StreamReader reader = new(path);
			string textToRead = reader.ReadToEnd();

			if (textToWrite.Equals(textToRead) ||
				textToRead.Equals(GetLastKnownContent(ApiVault.Get().GetTextEditor()))) return;

			if (IsMessageShown) return;
			IsMessageShown = true;
			await ShowMessage(item, textToRead);
		}
		catch (Exception e)
		{
			Log.Error(e, "Error while checking for changes");
		}

		IsMessageShown = false;
	}

	private static async Task ShowMessage(TabViewItem item, string textToRead)
	{
		ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(
			Translation.Get("Attention"),
			Translation.Get("ChangesDetected"),
			new SymbolIconSource() { Symbol = Symbol.ImportantFilled },
			primaryButtonContent: "Yes",
			closeButtonContent: "No"
		);


		if (result == ContentDialogResult.Primary)
		{
			ApiVault.Get().GetTextEditor().Document.Text = textToRead;
			SetLastKnownContent(ApiVault.Get().GetTextEditor(), textToRead);
			item.Header = item.Header.ToString().TrimEnd('*');
		}
		else
		{
			SetLastKnownContent(ApiVault.Get().GetTextEditor(), textToRead);
			item.Header = item.Header.ToString().TrimEnd('*') + "*";
		}
	}
}
