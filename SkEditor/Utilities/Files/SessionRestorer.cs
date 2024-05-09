using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Syntax;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;
public static class SessionRestorer
{
    private static string sessionFolder = Path.Combine(Path.GetTempPath(), "SkEditor", "Session");

    public static async void SaveSession()
    {
        List<TabViewItem> tabs = ApiVault.Get().GetTabView().TabItems
            .OfType<TabViewItem>()
            .Where(tab => tab.Content is TextEditor)
            .ToList();

        Directory.CreateDirectory(sessionFolder);

        foreach (TabViewItem tab in tabs)
        {
            string path = tab.Tag?.ToString().TrimEnd('*');
            string textToWrite = string.Empty;
            TextEditor editor = tab.Content as TextEditor;

            if (editor.Document.TextLength == 0) continue;

            if (string.IsNullOrEmpty(path))
            {
                string header = tab.Header.ToString().TrimEnd('*');
                path = Path.Combine(sessionFolder, header);
                textToWrite = editor.Text;
            }
            else
            {
                string name = Path.GetFileName(path);
                textToWrite = $"##SKEDITOR RESTORE:{path}##\n" + editor.Text;
                path = Path.Combine(sessionFolder, name);
            }

            await File.WriteAllTextAsync(path, textToWrite);
        }

        ApiVault.Get().OnClosed();
        ApiVault.Get().GetMainWindow().AlreadyClosed = true;
        ApiVault.Get().GetMainWindow().Close();
    }

    public static async Task<bool> RestoreSession()
    {
        bool filesAdded = false;

        if (!Directory.Exists(sessionFolder)) return filesAdded;

        foreach (string file in Directory.GetFiles(sessionFolder))
        {
            string path = file;
            string name = Path.GetFileName(file);
            string content = await File.ReadAllTextAsync(file);

            if (content.StartsWith("##SKEDITOR RESTORE"))
            {
                int endOfPathIndex = content.IndexOf('\n');
                if (endOfPathIndex > 0)
                {
                    path = content[19..endOfPathIndex];
                    path = path.Replace("##", string.Empty);
                    content = content[(endOfPathIndex + 1)..];
                }
            }

            TabViewItem tabItem = await FileBuilder.Build(name, path, content);
            TextEditor editor = tabItem.Content as TextEditor;

            (ApiVault.Get().GetTabView().TabItems as IList)?.Add(tabItem);
            SyntaxLoader.Load(editor);

            File.Delete(file);
            filesAdded = true;
        }

        return filesAdded;
    }
}
