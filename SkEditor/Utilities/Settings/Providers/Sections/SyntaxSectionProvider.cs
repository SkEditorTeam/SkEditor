using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Utils;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Models.UI;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using TextMateSharp.Grammars;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class SyntaxSectionProvider(
    AppSettings appSettings,
    ISyntaxService syntax,
    SyntaxThemeChoiceProvider themeChoiceProvider,
    SyntaxThemeSkriptChoiceProvider themeSkriptChoiceProvider,
    SyntaxGrammarChoiceProvider grammarChoiceProvider,
    SyntaxGrammarSkriptChoiceProvider grammarSkriptChoiceProvider,
    ILinkService links) : ISettingsSectionProvider
{
    public string ParentId => "Appearance";

    public SettingSection GetSection()
    {
        SettingSection syntaxSection = SettingSection
            .Create("Syntax", "Syntax highlighting".ToTranslatable())
            .WithIcon(Symbol.CodeTextEdit);

        ObservableCollection<SettingElement> settings = syntaxSection.Settings;
        settings.AddRange(CreateSkriptSection());
        settings.AddRange(CreateManagementSection());

        ExpandableSetting customRules = ExpandableSetting
            .Create("Custom rules".ToTranslatable())
            .WithDescription("Bind a custom grammar and theme to specific file extensions.".ToTranslatable())
            .WithIcon(Symbol.PuzzlePiece);

        List<SyntaxPairDto> customPairs = appSettings.SyntaxPairs
            .Where(p => !p.Extensions.Contains(".sk"))
            .ToList();

        foreach (SyntaxPairDto pair in customPairs)
        {
            customRules.Children.Add(CreateRuleElement(pair, customRules.Children));
        }

        customRules.Children.Add(
            ActionSetting
                .Create("Add new rule".ToTranslatable())
                .WithAction(TranslationKey.ButtonAdd, () =>
                {
                    SyntaxPairDto newPair = new([""], null, nameof(ThemeName.DarkPlus));
                    appSettings.SyntaxPairs.Add(newPair);
                    syntax.RefreshPairs();

                    SettingElement newElement = CreateRuleElement(newPair, customRules.Children);
                    customRules.Children.Insert(customRules.Children.Count - 1, newElement);
                })
        );

        settings.Add(customRules);

        return syntaxSection;
    }

    private SettingElement CreateRuleElement(SyntaxPairDto pair, ObservableCollection<SettingElement> parentCollection)
    {
        ActionSetting deleteAction = ActionSetting
            .Create("Delete".ToTranslatable())
            .WithIcon(Symbol.Delete);

        SettingElement ruleItem = CompoundSetting
            .Create("Rule".ToTranslatable())
            .WithIcon(Symbol.Link)
            .WithChildren(
                TextSetting
                    .Create("Extensions".ToTranslatable())
                    .WithProperty((
                        () => string.Join(", ", pair.Extensions),
                        val =>
                        {
                            if (string.IsNullOrWhiteSpace(val))
                            {
                                pair.Extensions = [];
                            }
                            else
                            {
                                pair.Extensions = val
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(e => e == "*" || e.StartsWith('.') ? e : $".{e}")
                                    .Where(e => !e.Equals(".sk", StringComparison.OrdinalIgnoreCase))
                                    .ToArray();
                            }
                            syntax.RefreshPairs();
                        }
                    )),
                ChoiceSetting<SyntaxGrammar?>
                    .Create("Grammar file".ToTranslatable())
                    .WithProperty((
                        () => syntax.AvailableGrammars.FirstOrDefault(g => g.FileName == pair.Grammar),
                        val =>
                        {
                            pair.Grammar = val?.FileName;
                            syntax.RefreshPairs();
                        }
                    ))
                    .WithProvider(grammarChoiceProvider),
                ChoiceSetting<SyntaxTheme?>
                    .Create("Theme file".ToTranslatable())
                    .WithProperty((
                        () => syntax.AvailableThemes.FirstOrDefault(t => t.FileName == pair.Theme),
                        val =>
                        {
                            pair.Theme = val?.FileName ?? pair.Theme;
                            syntax.RefreshPairs();
                        }
                    ))
                    .WithProvider(themeChoiceProvider),
                deleteAction
            );

        deleteAction.WithAction("Remove".ToTranslatable(), () =>
        {
            appSettings.SyntaxPairs.Remove(pair);
            parentCollection.Remove(ruleItem);
            (ruleItem as IDisposable)?.Dispose();
            syntax.RefreshPairs();
        });

        return ruleItem;
    }

    private List<SettingElement> CreateSkriptSection()
    {
        SyntaxPairDto? skriptPair = appSettings.SyntaxPairs.FirstOrDefault(p => p.Extensions.Contains(".sk"));
        if (skriptPair is null)
        {
            skriptPair = new SyntaxPairDto([".sk", ""], "skript.tmLanguage.json", "dark_skript.json");
            appSettings.SyntaxPairs.Insert(0, skriptPair);
        }

        ChoiceSetting<SyntaxGrammar?> skriptGrammarItem = ChoiceSetting<SyntaxGrammar?>
            .Create("Skript grammar".ToTranslatable())
            .WithProperty((
                () => syntax.AvailableGrammars.FirstOrDefault(g => g.FileName == skriptPair.Grammar),
                newGrammar =>
                {
                    skriptPair.Grammar = newGrammar?.FileName;
                    syntax.RefreshPairs();
                }
            ))
            .WithProvider(grammarSkriptChoiceProvider);

        BindableReactiveProperty<bool> isGrammarVisible = Observable.FromEvent(
                h => syntax.SyntaxesReloaded += h,
                h => syntax.SyntaxesReloaded -= h)
            .Select(_ => HasMultipleSkriptGrammars())
            .ToBindableReactiveProperty(HasMultipleSkriptGrammars());

        skriptGrammarItem.WithVisible(isGrammarVisible);

        return
        [
            CompoundSetting
                .Create("Skript syntax".ToTranslatable())
                .WithDescription("Choose the grammar and color scheme used for Skript code.".ToTranslatable())
                .WithIcon(Symbol.Color)
                .WithChildren(
                    skriptGrammarItem,
                    ChoiceSetting<SyntaxTheme?>
                        .Create("Skript theme".ToTranslatable())
                        .WithProperty((
                            () => syntax.AvailableThemes.FirstOrDefault(t => t.FileName == skriptPair.Theme),
                            newTheme =>
                            {
                                skriptPair.Theme = newTheme?.FileName ?? skriptPair.Theme;
                                syntax.RefreshPairs();
                            }
                        ))
                        .WithProvider(themeSkriptChoiceProvider)
                )
        ];
    }

    private bool HasMultipleSkriptGrammars()
    {
        return syntax.AvailableGrammars.Count(g => g.ScopeName == "source.skript") > 1;
    }

    private List<SettingElement> CreateManagementSection()
    {
        return
        [
            ActionSetting
                .Create("Syntax folder".ToTranslatable())
                .WithDescription("Open the folder containing syntax grammars and themes.".ToTranslatable())
                .WithIcon(Symbol.FolderOpen)
                .WithAction(TranslationKey.ButtonOpenFolder, () => links.OpenFolder(syntax.SyntaxFolderPath)),

            ActionSetting
                .Create("Reload syntaxes".ToTranslatable())
                .WithDescription("Reload files for newly added grammars and themes.".ToTranslatable())
                .WithIcon(Symbol.ArrowClockwise)
                .WithAction("Reload".ToTranslatable(), syntax.LoadSyntaxes)
        ];
    }
}