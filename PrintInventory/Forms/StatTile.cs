namespace PrintInventory.Forms;

// One summary tile at the top of the main screen: caption, big number, small detail text.
// Optionally a ring on the right side (for a percentage).
public class StatTile : DrawnControl
{
    private readonly string caption;
    private string valueText = "-";
    private string detailText = "";
    private Color accentColor = DarkTheme.AccentColor;
    private double? ringPercent; // null = no ring

    public StatTile(string caption)
    {
        this.caption = caption;
        Dock = DockStyle.Fill;
        Margin = new Padding(0, 0, 14, 0);
    }

    public void SetValue(string valueText, string detailText, Color accentColor, double? ringPercent = null)
    {
        this.valueText = valueText;
        this.detailText = detailText;
        this.accentColor = accentColor;
        this.ringPercent = ringPercent;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        DarkTheme.DrawCard(graphics, new RectangleF(0, 0, Width - 1, Height - 1), DarkTheme.CardColor, DarkTheme.BorderColor);

        // small glowing accent bar on the left edge
        using (var accentBrush = new SolidBrush(accentColor))
        using (var accentBar = DarkTheme.CreateRoundedRectangle(new RectangleF(0, 18, 4, 30), 2))
        {
            graphics.FillPath(accentBrush, accentBar);
        }

        // The caption uses the full width; the value and detail make room for the ring (if any).
        const int ringSize = 62;
        int ringSpace = ringPercent == null ? 0 : ringSize + 6;
        int textWidth = Width - 40 - ringSpace;

        DarkTheme.DrawText(graphics, caption, DarkTheme.SectionFont, new Rectangle(20, 16, Width - 36, 18), DarkTheme.MutedTextColor);
        DarkTheme.DrawText(graphics, valueText, DarkTheme.BigNumberFont, new Rectangle(18, 36, textWidth, 44), DarkTheme.TextColor);
        DarkTheme.DrawText(graphics, detailText, DarkTheme.SmallFont, new Rectangle(20, Height - 30, textWidth, 18), DarkTheme.MutedTextColor);

        if (ringPercent != null)
        {
            var circle = new RectangleF(Width - ringSize - 14, 38, ringSize, ringSize);
            DarkTheme.DrawRing(graphics, circle, 7, ringPercent.Value, accentColor);
        }
    }
}
