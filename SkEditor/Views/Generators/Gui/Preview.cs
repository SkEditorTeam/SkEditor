using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkEditor.Views.Generators.Gui.Generator;
public class Preview
{
    private const int slotWidth = 37;
    private const int slotHeight = 37;
    private const int slotsPerRow = 9;

    private static Dictionary<string, Bitmap> cachedImages = [];

    public static void Show()
    {
        int rows = GuiGenerator.Instance.CurrentRows;

        Bitmap guiBitmap = new(AssetLoader.Open(new Uri("avares://SkEditor/Assets/GUI/" + rows + ".png")));
        RenderTargetBitmap renderTargetBitmap = new(guiBitmap.PixelSize);
        DrawingContext ctx = renderTargetBitmap.CreateDrawingContext();
        ctx.DrawImage(guiBitmap, new Rect(0, 0, guiBitmap.PixelSize.Width, guiBitmap.PixelSize.Height));

        GuiGenerator.Instance.Items.ToList().ForEach((pair) => DrawItem(ctx, pair.Key, pair.Value));

        if (GuiGenerator.Instance.BackgroundItem != null)
        {
            int slots = GuiGenerator.Instance.CurrentRows * slotsPerRow;
            for (int i = 0; i < slots; i++)
            {
                if (GuiGenerator.Instance.Items.ContainsKey(i)) continue;
                DrawItem(ctx, i, GuiGenerator.Instance.BackgroundItem);
            }
        }

        ctx.Dispose();
        cachedImages.Clear();

        Image image = new()
        {
            Source = renderTargetBitmap,
            Stretch = Stretch.Uniform,
            Width = renderTargetBitmap.Size.Width,
            Height = renderTargetBitmap.Size.Height
        };
        RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.None);

        Flyout flyout = new()
        {
            Content = image,
            Placement = PlacementMode.Top,
            ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway
        };

        flyout.ShowAt(GuiGenerator.Instance.PreviewButton);
    }

    private static void DrawItem(DrawingContext ctx, int slot, Item item)
    {
        int row = slot / slotsPerRow;
        int column = slot % slotsPerRow;

        int slotX = 18 + (column * 41);
        int slotY = 41 * (row + 1);

        string itemImagePath = Path.Combine(GuiGenerator.Instance._itemPath, item.Name + ".png");

        if (!cachedImages.TryGetValue(itemImagePath, out Bitmap itemBitmap))
        {
            itemBitmap = new Bitmap(itemImagePath);
            cachedImages[itemImagePath] = itemBitmap;
        }

        ctx.DrawImage(itemBitmap, new Rect(slotX, slotY, slotWidth, slotHeight));
    }
}
