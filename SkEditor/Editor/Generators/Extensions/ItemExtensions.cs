using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Extensions;

public static class ItemExtensions
{
    extension(GuiItem)
    {
        public static GuiItem? FromViewModel(GuiItemViewModel? vm)
        {
            if (vm is null)
            {
                return null;
            }

            GuiItem item = new()
            {
                Id = vm.Id,
                DisplayName = vm.DisplayName,
                Amount = vm.Amount,
                GenerateExampleAction = vm.GenerateExampleAction,
                HideTooltip = vm.HideTooltip
            };

            return item;
        }
    }
}