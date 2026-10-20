namespace PrintInventory.Forms;

// A DateTimePicker that fits the dark theme.
// The normal DateTimePicker ignores BackColor and is always white. With UserPaint we draw
// the closed control ourselves (background, date text, arrow). Clicking it still opens
// the normal Windows calendar to pick a date.
public class DarkDateTimePicker : DateTimePicker
{
    public DarkDateTimePicker()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Format = DateTimePickerFormat.Custom;
        CustomFormat = "dd.MM.yyyy";
    }

    protected override void OnValueChanged(EventArgs eventargs)
    {
        base.OnValueChanged(eventargs);
        Invalidate(); // show the new date
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.Clear(DarkTheme.InputColor);

        using (var borderPen = new Pen(DarkTheme.BorderColor))
        {
            graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }

        var textBounds = new Rectangle(6, 0, Width - 32, Height);
        DarkTheme.DrawText(graphics, Value.ToString(CustomFormat), DarkTheme.NormalFont, textBounds, DarkTheme.TextColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        var arrowBounds = new Rectangle(Width - 26, 0, 20, Height);
        DarkTheme.DrawText(graphics, "▾", DarkTheme.NormalFont, arrowBounds, DarkTheme.MutedTextColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
