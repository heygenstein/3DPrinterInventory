using System.ComponentModel;

namespace PrintInventory.Forms;

// A row of "pill" buttons where exactly one is selected (like tabs), drawn in the dark theme.
public class SegmentedTabs : DrawnControl
{
    private readonly string[] titles;
    private int selectedIndex;
    private int hoveredIndex = -1;

    public event EventHandler? SelectedIndexChanged;

    public SegmentedTabs(params string[] titles)
    {
        this.titles = titles;
        Height = 40;
        Dock = DockStyle.Fill;
        BackColor = DarkTheme.CardColor;
        Cursor = Cursors.Hand;
        Margin = new Padding(0, 4, 0, 14);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // not saved by the Designer
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value == selectedIndex)
            {
                return;
            }
            selectedIndex = value;
            Invalidate();
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private RectangleF GetSegmentBounds(int index)
    {
        float segmentWidth = (Width - 1f) / titles.Length;
        return new RectangleF(index * segmentWidth, 0, segmentWidth, Height - 1);
    }

    private int GetIndexAt(int x)
    {
        float segmentWidth = (float)Width / titles.Length;
        return Math.Clamp((int)(x / segmentWidth), 0, titles.Length - 1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int index = GetIndexAt(e.X);
        if (index != hoveredIndex)
        {
            hoveredIndex = index;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hoveredIndex = -1;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        SelectedIndex = GetIndexAt(e.X);
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        // the outer "track"
        DarkTheme.DrawCard(graphics, new RectangleF(0, 0, Width - 1, Height - 1), DarkTheme.InputColor, DarkTheme.BorderColor, 10);

        for (int i = 0; i < titles.Length; i++)
        {
            RectangleF segment = RectangleF.Inflate(GetSegmentBounds(i), -4, -4);
            Color textColor = DarkTheme.MutedTextColor;

            if (i == selectedIndex)
            {
                DarkTheme.DrawCard(graphics, segment, Color.FromArgb(40, DarkTheme.AccentColor), DarkTheme.AccentColor, 7);
                textColor = DarkTheme.AccentColor;
            }
            else if (i == hoveredIndex)
            {
                DarkTheme.DrawCard(graphics, segment, DarkTheme.CardHoverColor, DarkTheme.CardHoverColor, 7);
                textColor = DarkTheme.TextColor;
            }

            DarkTheme.DrawText(graphics, titles[i], DarkTheme.NormalBoldFont, Rectangle.Round(segment), textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
