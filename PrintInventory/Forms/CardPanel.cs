namespace PrintInventory.Forms;

// A container with a rounded dark card as background. Other controls go inside it.
// The children should use DarkTheme.CardColor as BackColor, so they blend with the card.
public class CardPanel : Panel
{
    public CardPanel()
    {
        SetStyle(ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);
        BackColor = DarkTheme.WindowColor;
        Padding = new Padding(18); // keeps the children away from the round corners
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);
        DarkTheme.DrawCard(e.Graphics, new RectangleF(0, 0, Width - 1, Height - 1),
            DarkTheme.CardColor, DarkTheme.BorderColor);
    }
}
