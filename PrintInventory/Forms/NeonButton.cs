using System.Drawing.Drawing2D;

namespace PrintInventory.Forms;

// A flat, rounded button for the dark theme.
// Primary = filled with the neon accent color. Secondary = dark with an outline.
public class NeonButton : Button
{
    private readonly bool isPrimary;
    private bool isHovered;

    public NeonButton(string text, bool isPrimary)
    {
        this.isPrimary = isPrimary;
        Text = text;
        Font = DarkTheme.NormalBoldFont;
        Size = new Size(TextRenderer.MeasureText(text, Font).Width + 36, 38);
        Margin = new Padding(10, 0, 0, 0);
        Cursor = Cursors.Hand;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = DarkTheme.WindowColor;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    // When the text changes later (e.g. "Arşivle" -> "Arşivden çıkar"), make the button fit it.
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Width = TextRenderer.MeasureText(Text, Font).Width + 36;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        isHovered = true;
        Invalidate(); // = "please draw me again"
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
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Parent?.BackColor ?? DarkTheme.WindowColor);

        var bounds = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        Color fillColor;
        Color borderColor;
        Color textColor;
        if (isPrimary)
        {
            fillColor = isHovered ? DarkTheme.Blend(DarkTheme.AccentColor, Color.White, 0.25) : DarkTheme.AccentColor;
            borderColor = fillColor;
            textColor = DarkTheme.WindowColor;
        }
        else
        {
            fillColor = isHovered ? DarkTheme.CardHoverColor : DarkTheme.CardColor;
            borderColor = isHovered ? DarkTheme.AccentColor : DarkTheme.BorderColor;
            textColor = DarkTheme.TextColor;
        }

        if (isPrimary && isHovered)
        {
            DarkTheme.DrawGlow(graphics, bounds, DarkTheme.AccentColor, 8);
        }
        DarkTheme.DrawCard(graphics, bounds, fillColor, borderColor, 8);
        DarkTheme.DrawText(graphics, Text, Font, ClientRectangle, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
