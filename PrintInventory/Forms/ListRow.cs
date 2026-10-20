using System.ComponentModel;

namespace PrintInventory.Forms;

// One clickable row in a list: a colored dot, title + subtitle on the left,
// a value + small text on the right, and (optionally) a ✕ delete button on hover.
//
//   ( • )  eSun PLA                                 3 makara
//          Black · 1,75 mm · 1,24 g/cm³          215 °C / 60 °C   ✕
public class ListRow : DrawnControl
{
    private const int DeleteZoneWidth = 44;

    private bool isHovered;
    private bool isDeleteHovered;

    // [DesignerSerializationVisibility(Hidden)] on the properties: "the Visual Studio Designer
    // should not save these". We build the UI in code, so there is nothing to save.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Subtitle { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string RightText { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string RightSubText { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color RightTextColor { get; set; } = DarkTheme.TextColor;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color DotColor { get; set; } = DarkTheme.AccentColor;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DotGlow { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowDeleteButton { get; set; }

    // We use our own events (not Click), so a click on ✕ does not also count as a row click.
    public event EventHandler? RowClicked;
    public event EventHandler? DeleteClicked;

    public ListRow()
    {
        Height = 62;
        Margin = new Padding(0, 0, 0, 8);
        BackColor = DarkTheme.CardColor;
        Cursor = Cursors.Hand;
    }

    private bool IsInDeleteZone(int x)
    {
        return ShowDeleteButton && x > Width - DeleteZoneWidth;
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
        isDeleteHovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        bool overDelete = IsInDeleteZone(e.X);
        if (overDelete != isDeleteHovered)
        {
            isDeleteHovered = overDelete;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (IsInDeleteZone(e.X))
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            RowClicked?.Invoke(this, EventArgs.Empty);
        }
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        var card = new RectangleF(0, 0, Width - 1, Height - 1);
        Color fillColor = isHovered ? DarkTheme.CardHoverColor : DarkTheme.InputColor;
        Color borderColor = isHovered ? DarkTheme.AccentColor : DarkTheme.BorderColor;
        DarkTheme.DrawCard(graphics, card, fillColor, borderColor, 10);

        // dot (with a thin outline, so black filament is visible too)
        var dotCenter = new PointF(24, Height / 2f);
        DarkTheme.DrawDot(graphics, dotCenter, 14, DotColor, DotGlow);
        using (var outlinePen = new Pen(DarkTheme.MutedTextColor))
        {
            graphics.DrawEllipse(outlinePen, dotCenter.X - 7, dotCenter.Y - 7, 14, 14);
        }

        int rightEdge = Width - 16 - (ShowDeleteButton ? DeleteZoneWidth - 12 : 0);
        int rightWidth = Math.Min(170, Width / 3);
        int textLeft = 46;
        int textWidth = rightEdge - rightWidth - textLeft - 8;

        DarkTheme.DrawText(graphics, Title, DarkTheme.NormalBoldFont, new Rectangle(textLeft, 11, textWidth, 20), DarkTheme.TextColor);
        DarkTheme.DrawText(graphics, Subtitle, DarkTheme.SmallFont, new Rectangle(textLeft, 34, textWidth, 16), DarkTheme.MutedTextColor);

        var rightTop = new Rectangle(rightEdge - rightWidth, 11, rightWidth, 20);
        var rightBottom = new Rectangle(rightEdge - rightWidth, 34, rightWidth, 16);
        DarkTheme.DrawText(graphics, RightText, DarkTheme.NormalBoldFont, rightTop, RightTextColor, TextFormatFlags.Right);
        DarkTheme.DrawText(graphics, RightSubText, DarkTheme.SmallFont, rightBottom, DarkTheme.MutedTextColor, TextFormatFlags.Right);

        if (ShowDeleteButton && isHovered)
        {
            Color deleteColor = isDeleteHovered ? DarkTheme.WarningColor : DarkTheme.MutedTextColor;
            var deleteBounds = new Rectangle(Width - DeleteZoneWidth, 0, DeleteZoneWidth - 6, Height);
            DarkTheme.DrawText(graphics, "✕", DarkTheme.NormalBoldFont, deleteBounds, deleteColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
