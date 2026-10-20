namespace PrintInventory.Forms;

// The bar at the top: glowing logo dot, app name, printer chip, and buttons on the right.
public class HeaderBar : DrawnControl
{
    private string printerText = "";

    // The main form puts its buttons in here.
    public FlowLayoutPanel ButtonsPanel { get; } = new FlowLayoutPanel
    {
        Dock = DockStyle.Right,
        AutoSize = true,
        WrapContents = false,
        BackColor = DarkTheme.WindowColor,
        Padding = new Padding(0, 13, 20, 0)
    };

    public HeaderBar()
    {
        Height = 64;

        // Thin line at the bottom: a 1-pixel panel, docked first (SendToBack), so it gets the
        // full width and the buttons panel is placed above it, not over it.
        var bottomLine = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = DarkTheme.BorderColor };
        Controls.Add(ButtonsPanel);
        Controls.Add(bottomLine);
        bottomLine.SendToBack();
    }

    public void SetPrinterText(string text)
    {
        printerText = text;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        int centerY = Height / 2;
        DarkTheme.DrawDot(graphics, new PointF(28, centerY), 10, DarkTheme.AccentColor, withGlow: true);

        const string title = "BASKI ENVANTERİ";
        Size titleSize = TextRenderer.MeasureText(title, DarkTheme.TitleFont);
        var titleBounds = new Rectangle(44, centerY - titleSize.Height / 2, titleSize.Width, titleSize.Height);
        DarkTheme.DrawText(graphics, title, DarkTheme.TitleFont, titleBounds, DarkTheme.TextColor);

        if (printerText != "")
        {
            DrawPrinterChip(graphics, titleBounds.Right + 18, centerY);
        }
    }

    // A small rounded "chip": green dot + printer name.
    private void DrawPrinterChip(Graphics graphics, int left, int centerY)
    {
        Size textSize = TextRenderer.MeasureText(printerText, DarkTheme.SmallFont);
        var chip = new RectangleF(left, centerY - 13, textSize.Width + 36, 26);
        DarkTheme.DrawCard(graphics, chip, DarkTheme.CardColor, DarkTheme.BorderColor, 13);
        DarkTheme.DrawDot(graphics, new PointF(chip.X + 14, centerY), 7, DarkTheme.SuccessColor, withGlow: true);

        var textBounds = new Rectangle((int)chip.X + 24, centerY - textSize.Height / 2, textSize.Width + 4, textSize.Height);
        DarkTheme.DrawText(graphics, printerText, DarkTheme.SmallFont, textBounds, DarkTheme.MutedTextColor);
    }
}
