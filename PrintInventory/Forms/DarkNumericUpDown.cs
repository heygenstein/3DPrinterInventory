namespace PrintInventory.Forms;

// A NumericUpDown for the dark theme.
// Windows always draws the small up/down arrows light, so we hide them. The normal control
// then leaves a white strip where the arrows were; this class paints that area dark instead.
// Typing, the mouse wheel and the Up/Down keys still change the value.
public class DarkNumericUpDown : NumericUpDown
{
    public DarkNumericUpDown()
    {
        BackColor = DarkTheme.InputColor;
        ForeColor = DarkTheme.TextColor;
        BorderStyle = BorderStyle.FixedSingle;
        Controls[0].Visible = false; // Controls[0] = the up/down arrows, Controls[1] = the text box
    }

    // We draw only the background and the border; the text box inside draws the number.
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(DarkTheme.InputColor);
        using var borderPen = new Pen(DarkTheme.BorderColor);
        e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
    }
}
