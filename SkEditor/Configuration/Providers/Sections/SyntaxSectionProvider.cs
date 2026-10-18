using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Utils;
using FluentIcons.Common;
using R3;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Configuration.Providers.Choice;
using SkEditor.Core.System;
using SkEditor.Core.Localization;
using SkEditor.Editor.Syntax;
using SkEditor.Editor.Syntax.Models;
using TextMateSharp.Grammars;

namespace SkEditor.Configuration.Providers.Sections;

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
            .Create("Syntax", TranslationKey.SettingsSectionSyntaxHighlighting)
            .WithIcon(Symbol.CodeTextEdit)
            .WithOrder(20);

        ObservableCollection<SettingElement> settings = syntaxSection.Settings;
        settings.AddRange(CreateSkriptSection());
        settings.AddRange(CreateManagementSection());

        ExpandableSetting customRules = ExpandableSetting
            .Create(TranslationKey.SettingsSyntaxCustomRules)
            .WithDescription(TranslationKey.SettingsSyntaxCustomRulesDescription)
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
                .Create(TranslationKey.SettingsSyntaxCustomRulesAdd)
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
            .Create(TranslationKey.SettingsSyntaxCustomRulesDelete)
            .WithIcon(Symbol.Delete);

        SettingElement ruleItem = CompoundSetting
            .Create(TranslationKey.SettingsSyntaxCustomRulesRule)
            .WithIcon(Symbol.Link)
            .WithChildren(
                TextSetting
                    .Create(TranslationKey.SettingsSyntaxCustomRulesRuleExtensions)
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
                    .Create(TranslationKey.SettingsSyntaxCustomRulesRuleGrammarFile)
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
                    .Create(TranslationKey.SettingsSyntaxCustomRulesRuleThemeFile)
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

        deleteAction.WithAction(TranslationKey.SettingsSyntaxCustomRulesDelete, () =>
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
            .Create(TranslationKey.SettingsSyntaxSkriptGrammar)
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
                .Create(TranslationKey.SettingsSyntaxSkriptSyntax)
                .WithDescription(TranslationKey.SettingsSyntaxSkriptSyntaxDescription)
                .WithIcon(Symbol.Color)
                .WithChildren(
                    skriptGrammarItem,
                    ChoiceSetting<SyntaxTheme?>
                        .Create(TranslationKey.SettingsSyntaxSkriptTheme)
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
                .Create(TranslationKey.SettingsSyntaxFolder)
                .WithDescription(TranslationKey.SettingsSyntaxFolderDescription)
                .WithIcon(Symbol.FolderOpen)
                .WithAction(TranslationKey.ButtonOpenFolder, () => links.OpenFolder(syntax.SyntaxFolderPath)),

            ActionSetting
                .Create(TranslationKey.SettingsSyntaxReload)
                .WithDescription(TranslationKey.SettingsSyntaxReloadDescription)
                .WithIcon(Symbol.ArrowClockwise)
                .WithAction(TranslationKey.ButtonReload, syntax.LoadSyntaxes)
        ];
    }
}