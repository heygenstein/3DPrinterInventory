using System.Drawing.Drawing2D;
using PrintInventory.Data;

namespace PrintInventory.Forms;

// One model in the gallery: its preview picture, name, estimated grams and time.
public class ModelTile : DrawnControl
{
    private const int ImageHeight = 124;

    private Image? thumbnail;
    private string modelName = "";
    private string details = "";
    private bool isFileMissing;
    private bool isHovered;

    public event EventHandler? TileClicked;

    public ModelTile()
    {
        Size = new Size(196, 204);
        Margin = new Padding(0, 0, 12, 12);
        BackColor = DarkTheme.CardColor;
        Cursor = Cursors.Hand;
    }

    public void SetModel(PrintModel model)
    {
        thumbnail?.Dispose();
        thumbnail = ImageLoader.LoadThumbnail(model.PreviewImagePath, 260);
        modelName = model.Name;
        details = $"{model.EstimatedGrams:0.#} g  ·  {DisplayText.FormatDuration(model.EstimatedPrintTimeMinutes)}";
        isFileMissing = !string.IsNullOrEmpty(model.FilePath) && !File.Exists(model.FilePath);
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        isHovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        TileClicked?.Invoke(this, EventArgs.Empty);
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        var card = new RectangleF(0, 0, Width - 1, Height - 1);
        DarkTheme.DrawCard(graphics, card, isHovered ? DarkTheme.CardHoverColor : DarkTheme.InputColor,
            isHovered ? DarkTheme.AccentColor : DarkTheme.BorderColor);

        var imageArea = new RectangleF(10, 10, Width - 21, ImageHeight);
        DrawPicture(graphics, imageArea);

        int textTop = (int)imageArea.Bottom + 10;
        DarkTheme.DrawText(graphics, modelName, DarkTheme.NormalBoldFont, new Rectangle(12, textTop, Width - 24, 20), DarkTheme.TextColor);
        DarkTheme.DrawText(graphics, details, DarkTheme.SmallFont, new Rectangle(12, textTop + 23, Width - 24, 16), DarkTheme.MutedTextColor);
        if (isFileMissing)
        {
            DarkTheme.DrawText(graphics, "dosya bulunamadı", DarkTheme.SmallFont, new Rectangle(12, textTop + 40, Width - 24, 16), DarkTheme.WarningColor);
        }
    }

    // The picture fits inside the area (aspect ratio kept), with round corners.
    // Without a picture we show a placeholder.
    private void DrawPicture(Graphics graphics, RectangleF area)
    {
        using GraphicsPath roundedArea = DarkTheme.CreateRoundedRectangle(area, 8);
        using (var backgroundBrush = new SolidBrush(DarkTheme.TrackColor))
        {
            graphics.FillPath(backgroundBrush, roundedArea);
        }

        if (thumbnail == null)
        {
            DarkTheme.DrawText(graphics, "◇  3D", DarkTheme.RingValueFont, Rectangle.Round(area), DarkTheme.MutedTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        float scale = Math.Min(area.Width / thumbnail.Width, area.Height / thumbnail.Height);
        float width = thumbnail.Width * scale;
        float height = thumbnail.Height * scale;
        var imageBounds = new RectangleF(area.X + (area.Width - width) / 2, area.Y + (area.Height - height) / 2, width, height);

        // SetClip: draw only inside the rounded area, so the picture gets round corners too.
        graphics.SetClip(roundedArea);
        graphics.DrawImage(thumbnail, imageBounds);
        graphics.ResetClip();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            thumbnail?.Dispose();
        }
        base.Dispose(disposing);
    }
}
