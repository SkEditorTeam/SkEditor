using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SkEditor.Views.Generators.Gui;

public class Preview
{
    private const int SlotWidth = 37;
    private const int SlotHeight = 37;
    private const int SlotsPerRow = 9;

    private static readonly Dictionary<string, Bitmap> CachedImages = [];

    public static void Show()
    {
        if (GuiGenerator.Instance is null)
        {
            return;
        }
        
        int rows = GuiGenerator.Instance.CurrentRows;

        Bitmap guiBitmap = new(AssetLoader.Open(new Uri("avares://SkEditor/Assets/GUI/" + rows + ".png")));
        RenderTargetBitmap renderTargetBitmap = new(guiBitmap.PixelSize);
        DrawingContext ctx = renderTargetBitmap.CreateDrawingContext();
        ctx.DrawImage(guiBitmap, new Rect(0, 0, guiBitmap.PixelSize.Width, guiBitmap.PixelSize.Height));

        GuiGenerator.Instance.Items.ToList().ForEach(pair => DrawItem(ctx, pair.Key, pair.Value));

        if (GuiGenerator.Instance.BackgroundItem != null)
        {
            int slots = GuiGenerator.Instance.CurrentRows * SlotsPerRow;
            for (int i = 0; i < slots; i++)
            {
                if (GuiGenerator.Instance.Items.ContainsKey(i))
                {
                    continue;
                }

                DrawItem(ctx, i, GuiGenerator.Instance.BackgroundItem);
            }
        }

        ctx.Dispose();
        CachedImages.Clear();

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
        int row = slot / SlotsPerRow;
        int column = slot % SlotsPerRow;

        int slotX = 18 + (column * 41);
        int slotY = 41 * (row + 1);

        if (GuiGenerator.Instance is null) return;

        string itemImagePath = Path.Combine(GuiGenerator.Instance.ItemPath, item.Name + ".png");

        if (!CachedImages.TryGetValue(itemImagePath, out Bitmap? itemBitmap))
        {
            itemBitmap = new Bitmap(itemImagePath);
            CachedImages[itemImagePath] = itemBitmap;
        }

        ctx.DrawImage(itemBitmap, new Rect(slotX, slotY, SlotWidth, SlotHeight));
    }
}