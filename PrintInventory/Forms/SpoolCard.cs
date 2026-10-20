using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// One spool in the "spool rack": a ring in the filament's color that shows how full it is.
// Low-stock spools get a red glow and an "AZ" (low) badge.
public class SpoolCard : DrawnControl
{
    private const int GlowMargin = 5; // free space around the card for the glow
    private const int RingSize = 124;

    private readonly EaseOutAnimation fillAnimation = new EaseOutAnimation();
    private SpoolStatus? status;
    private bool isHovered;

    public SpoolCard()
    {
        Size = new Size(200, 252);
        Margin = new Padding(0, 0, 10, 10);
        fillAnimation.ProgressChanged += FillAnimation_ProgressChanged;
    }

    public SpoolStatus? Status => status;

    // animate = true: the ring fills up and the grams count up (used when the app starts).
    public void SetStatus(SpoolStatus newStatus, bool animate)
    {
        status = newStatus;
        if (animate)
        {
            fillAnimation.Start();
        }
        Invalidate();
    }

    private void FillAnimation_ProgressChanged(object? sender, EventArgs e)
    {
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

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);
        if (status == null)
        {
            return;
        }

        Spool spool = status.Spool;
        var card = new RectangleF(GlowMargin, GlowMargin, Width - 2 * GlowMargin - 1, Height - 2 * GlowMargin - 1);

        // 1) Card background, border and (for low stock) a red glow
        Color borderColor = DarkTheme.BorderColor;
        if (status.IsLowStock)
        {
            borderColor = DarkTheme.WarningColor;
            DarkTheme.DrawGlow(graphics, card, DarkTheme.WarningColor);
        }
        else if (isHovered)
        {
            borderColor = DarkTheme.AccentColor;
        }
        Color fillColor = isHovered ? DarkTheme.CardHoverColor : DarkTheme.CardColor;
        DarkTheme.DrawCard(graphics, card, fillColor, borderColor);

        // 2) Spool number (top left) and LOW badge (top right)
        int left = (int)card.X + 14;
        int top = (int)card.Y + 12;
        DarkTheme.DrawText(graphics, $"#{spool.Id}", DarkTheme.SmallFont, new Rectangle(left, top, 60, 16), DarkTheme.MutedTextColor);
        if (status.IsLowStock)
        {
            DrawLowBadge(graphics, (int)card.Right - 14, top - 2);
        }

        // 3) The ring, in the filament color
        float ringX = (Width - RingSize) / 2f;
        float ringY = card.Y + 34;
        var ringBounds = new RectangleF(ringX, ringY, RingSize, RingSize);
        Color filamentColor = HexColor.ToColor(spool.Filament.ColorHex);
        DarkTheme.DrawRing(graphics, ringBounds, 12, status.RemainingPercent * fillAnimation.Progress, DarkTheme.GetVisibleColor(filamentColor));

        // 4) Text in the middle of the ring: grams and percent
        Rectangle ringRect = Rectangle.Round(ringBounds);
        int middleY = ringRect.Y + ringRect.Height / 2;
        Color gramsColor = status.IsLowStock ? DarkTheme.WarningColor : DarkTheme.TextColor;
        DarkTheme.DrawText(graphics, $"{status.RemainingGrams * fillAnimation.Progress:0} g", DarkTheme.RingValueFont,
            new Rectangle(ringRect.X, middleY - 22, ringRect.Width, 28), gramsColor, TextFormatFlags.HorizontalCenter);
        DarkTheme.DrawText(graphics, DisplayText.Percent(status.RemainingPercent * fillAnimation.Progress), DarkTheme.SmallFont,
            new Rectangle(ringRect.X, middleY + 6, ringRect.Width, 16), DarkTheme.MutedTextColor, TextFormatFlags.HorizontalCenter);

        // 5) Brand, and "material · color" with a small dot in the real filament color
        int textTop = ringRect.Bottom + 12;
        int textWidth = (int)card.Width - 20;
        int textLeft = (int)card.X + 10;
        DarkTheme.DrawText(graphics, spool.Filament.Brand, DarkTheme.NormalBoldFont,
            new Rectangle(textLeft, textTop, textWidth, 20), DarkTheme.TextColor, TextFormatFlags.HorizontalCenter);
        DrawMaterialLine(graphics, spool.Filament, filamentColor, textTop + 24);
    }

    private void DrawLowBadge(Graphics graphics, int right, int top)
    {
        const string text = "AZ";
        Size textSize = TextRenderer.MeasureText(text, DarkTheme.SectionFont);
        var badge = new RectangleF(right - textSize.Width - 16, top, textSize.Width + 16, 20);
        DarkTheme.DrawCard(graphics, badge, Color.FromArgb(50, DarkTheme.WarningColor), DarkTheme.WarningColor, 10);
        DarkTheme.DrawText(graphics, text, DarkTheme.SectionFont, Rectangle.Round(badge), DarkTheme.WarningColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private void DrawMaterialLine(Graphics graphics, Filament filament, Color filamentColor, int top)
    {
        string text = $"{DisplayText.ForMaterial(filament.Material)} · {filament.ColorName}";
        Size textSize = TextRenderer.MeasureText(text, DarkTheme.SmallFont);
        const int dotSize = 10;
        const int gap = 6;

        // center "dot + text" together
        int totalWidth = Math.Min(dotSize + gap + textSize.Width, Width - 30);
        int startX = (Width - totalWidth) / 2;

        var dotCenter = new PointF(startX + dotSize / 2f, top + textSize.Height / 2f);
        DarkTheme.DrawDot(graphics, dotCenter, dotSize, filamentColor, withGlow: false);
        using (var outlinePen = new Pen(DarkTheme.MutedTextColor))
        {
            graphics.DrawEllipse(outlinePen, dotCenter.X - dotSize / 2f, dotCenter.Y - dotSize / 2f, dotSize, dotSize);
        }

        var textBounds = new Rectangle(startX + dotSize + gap, top, totalWidth - dotSize - gap, textSize.Height);
        DarkTheme.DrawText(graphics, text, DarkTheme.SmallFont, textBounds, DarkTheme.MutedTextColor);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            fillAnimation.Dispose();
        }
        base.Dispose(disposing);
    }
}
