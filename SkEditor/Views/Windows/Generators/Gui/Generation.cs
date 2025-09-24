using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.API;

namespace SkEditor.Views.Windows.Generators.Gui;

public class Generation
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static GuiGenerator? _guiGen;

    public static void Generate()
    {
        _guiGen = GuiGenerator.Instance;
        if (_guiGen == null) return;

        StringBuilder code = new();

        string functionName = string.IsNullOrEmpty(_guiGen.FunctionNameTextBox.Text)
            ? "openGUI"
            : _guiGen.FunctionNameTextBox.Text.Trim().Replace(" ", "_");

        if (string.IsNullOrWhiteSpace(_guiGen.TitleTextBox.Text))
        {
            _guiGen.TitleTextBox.Text = "GUI";
        }

        TextEditor? editor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (editor == null)
        {
            return;
        }

        int offset = editor.CaretOffset;
        DocumentLine line = editor.Document.GetLineByOffset(offset);

        if (!string.IsNullOrEmpty(editor.Document.GetText(line.Offset, line.Length)))
        {
            code.Append("\n\n");
        }

        code.Append($"function {functionName}(p: player):");

        code.Append(_guiGen.UseSkriptGuiCheckBox.IsChecked == false ? GetSkriptCode() : GetSkriptGuiCode());

        editor.Document.Insert(offset, code.ToString());
        editor.CaretOffset = offset + code.Length;
    }

    private static string GetSkriptCode()
    {
        if (_guiGen == null) return string.Empty;
        
        int rowQuantity = _guiGen.CurrentRows;

        StringBuilder code = new();
        code.Append(
            $"\n\tset {{_gui}} to chest inventory with {rowQuantity} rows named \"{_guiGen.TitleTextBox.Text}\"\n");

        if (_guiGen.BackgroundItem != null)
        {
            code.Append($"\n\tset slot (integers between 0 and {rowQuantity * 9}) of {{_gui}} to ");
            code.Append(_guiGen.BackgroundItem.Name.Replace("_", " "));
            AppendCustomName(code, _guiGen.BackgroundItem);
            AppendLore(code, _guiGen.BackgroundItem);
            AppendCustomModelData(code, _guiGen.BackgroundItem);
        }

        List<KeyValuePair<int, Item>> sortedItems = _guiGen.Items.ToList();
        sortedItems.Sort((x, y) => x.Key.CompareTo(y.Key));

        foreach (KeyValuePair<int, Item> item in sortedItems)
        {
            code.Append($"\n\tset slot {item.Key} of {{_gui}} to {item.Value.Name.Replace("_", " ")}");
            AppendCustomName(code, item.Value);
            AppendLore(code, item.Value);
            AppendCustomModelData(code, item.Value);
        }

        code.Append("\n\topen {_gui} to {_p}");

        code.Append("\n\non inventory click:");
        code.Append($"\n\tname of event-inventory is \"{_guiGen.TitleTextBox.Text}\"");
        code.Append("\n\tcancel event");
        code.Append("\n\tevent-inventory is not player's inventory");
        bool first = true;
        foreach (KeyValuePair<int, Item> item in sortedItems.Where(pair => pair.Value.HaveExampleAction))
        {
            code.Append($"\n\t{(first ? "" : "else ")}if clicked slot is {item.Key}:");
            code.Append($"\n\t\tsend \"You clicked on slot {item.Key}\"");
            first = false;
        }

        return code.ToString();
    }

    private static string GetSkriptGuiCode()
    {
        if (_guiGen == null) return string.Empty;
        
        int rowQuantity = _guiGen.CurrentRows;

        StringBuilder code = new();
        code.Append(
            $"\n\tcreate a gui with virtual chest inventory with {rowQuantity} rows named \"{_guiGen.TitleTextBox.Text}\"");

        if (_guiGen.BackgroundItem != null)
        {
            code.Append(GetBackgroundCode());
        }

        code.Append(':');

        if (_guiGen.BackgroundItem != null)
        {
            code.Append($"\n\t\tmake gui slot \"=\" with {_guiGen.BackgroundItem.Name.Replace("_", " ")}");
            AppendCustomName(code, _guiGen.BackgroundItem);
            AppendLore(code, _guiGen.BackgroundItem);
            AppendCustomModelData(code, _guiGen.BackgroundItem);
        }

        List<KeyValuePair<int, Item>> sortedItems = _guiGen.Items.ToList();
        sortedItems.Sort((x, y) => x.Key.CompareTo(y.Key));

        foreach (KeyValuePair<int, Item> pair in sortedItems)
        {
            code.Append($"\n\t\tmake gui slot {pair.Key} with {pair.Value.Name.Replace("_", " ")}");
            AppendCustomName(code, pair.Value);
            AppendLore(code, pair.Value);
            AppendCustomModelData(code, pair.Value);
            if (!pair.Value.HaveExampleAction)
            {
                continue;
            }

            code.Append(':');
            code.Append($"\n\t\t\tsend \"You clicked on slot {pair.Key}\"");
        }

        code.Append("\n\topen the last gui for {_p}");

        return code.ToString();
    }


    #region Utils

    private static void AppendLore(StringBuilder code, Item item)
    {
        if (item.Lore.Count <= 0)
        {
            return;
        }

        code.Append(" with lore");

        if (item.Lore.Count == 1)
        {
            code.Append($" \"{item.Lore[0]}\"");
            return;
        }

        for (int i = 0; i < item.Lore.Count; i++)
        {
            code.Append(i == item.Lore.Count - 1 ? $" and \"{item.Lore[i]}\"" : $" \"{item.Lore[i]}\",");
        }
    }

    private static void AppendCustomName(StringBuilder code, Item item)
    {
        if (item.HaveCustomName)
        {
            code.Append($" named \"{item.CustomName}\"");
        }
    }

    private static void AppendCustomModelData(StringBuilder code, Item item)
    {
        if (item.HaveCustomModelData)
        {
            code.Append($" with custom model data {item.CustomModelData}");
        }
    }

    private static string GetBackgroundCode()
    {
        if (_guiGen == null) return string.Empty;
        
        int rowQuantity = _guiGen.CurrentRows;
        int slots = rowQuantity * 9;

        List<List<char>> rows = Enumerable.Range(0, rowQuantity).Select(_ => new List<char>()).ToList();
        HashSet<char> usedChars = [];

        for (int i = 0; i < slots; i++)
        {
            if (!_guiGen.Items.ContainsKey(i))
            {
                rows[i / 9].Add('=');
            }
            else
            {
                char randomChar =
                    !usedChars.Contains(_guiGen.Items.First(pair => pair.Key == i).Value.Name.ElementAt(0))
                        ? _guiGen.Items.First(pair => pair.Key == i).Value.Name.ElementAt(0)
                        : GenerateRandomChar(usedChars);

                usedChars.Add(randomChar);
                rows[i / 9].Add(randomChar);
            }
        }

        StringBuilder code = new(" and shape");

        if (rowQuantity == 1)
        {
            code.Append($" \"{string.Join("", rows[0])}\"");
        }
        else
        {
            for (int i = 0; i < rowQuantity; i++)
            {
                code.Append(i == rowQuantity - 1
                    ? $" and \"{string.Join("", rows[i])}\""
                    : $" \"{string.Join("", rows[i])}\",");
            }
        }

        return code.ToString();
    }

    private static char GenerateRandomChar(HashSet<char> usedChars)
    {
        Random random = new();
        char randomChar;
        do
        {
            randomChar = Chars[random.Next(Chars.Length)];
        } while (usedChars.Contains(randomChar));

        return randomChar;
    }

    #endregion
}