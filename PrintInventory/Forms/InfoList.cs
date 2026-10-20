namespace PrintInventory.Forms;

// One "label ..... value" line in an InfoList.
public class InfoRow
{
    public string Label { get; }
    public string Value { get; }
    public Color ValueColor { get; }

    public InfoRow(string label, string value, Color valueColor)
    {
        Label = label;
        Value = value;
        ValueColor = valueColor;
    }

    public InfoRow(string label, string value) : this(label, value, DarkTheme.TextColor)
    {
    }
}

// A drawn list of label/value lines with thin separators (like a spec sheet).
public class InfoList : DrawnControl
{
    public const int RowHeight = 30;

    private List<InfoRow> rows = new List<InfoRow>();

    public InfoList()
    {
        BackColor = DarkTheme.CardColor;
        Dock = DockStyle.Fill;
    }

    public void SetRows(List<InfoRow> newRows)
    {
        rows = newRows;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.Clear(BackColor);

        using var separatorPen = new Pen(DarkTheme.BorderColor);
        for (int i = 0; i < rows.Count; i++)
        {
            int top = i * RowHeight;
            var bounds = new Rectangle(0, top, Width, RowHeight);
            DarkTheme.DrawText(graphics, rows[i].Label, DarkTheme.NormalFont, bounds, DarkTheme.MutedTextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            DarkTheme.DrawText(graphics, rows[i].Value, DarkTheme.NormalBoldFont, bounds, rows[i].ValueColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            graphics.DrawLine(separatorPen, 0, top + RowHeight - 1, Width, top + RowHeight - 1);
        }
    }
}
