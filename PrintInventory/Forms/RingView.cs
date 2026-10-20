namespace PrintInventory.Forms;

// A big ring with a value in the middle, e.g. "673 g" and "67 %" under it.
public class RingView : DrawnControl
{
    private const float Thickness = 16;

    private readonly EaseOutAnimation fillAnimation = new EaseOutAnimation();

    private double percent;
    private Color ringColor = DarkTheme.AccentColor;
    private string valueText = "";
    private string subText = "";
    private Color valueColor = DarkTheme.TextColor;

    public RingView()
    {
        BackColor = DarkTheme.CardColor;
        Dock = DockStyle.Fill;
        fillAnimation.ProgressChanged += FillAnimation_ProgressChanged;
    }

    private void FillAnimation_ProgressChanged(object? sender, EventArgs e)
    {
        Invalidate();
    }

    public void SetValue(double newPercent, Color newRingColor, string newValueText, string newSubText, Color newValueColor)
    {
        percent = newPercent;
        ringColor = newRingColor;
        valueText = newValueText;
        subText = newSubText;
        valueColor = newValueColor;
        fillAnimation.Start(); // the ring fills up every time a new value is shown
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        int size = Math.Min(Width, Height) - 8;
        if (size <= 0)
        {
            return;
        }
        var circle = new RectangleF((Width - size) / 2f, (Height - size) / 2f, size, size);
        DarkTheme.DrawRing(graphics, circle, Thickness, percent * fillAnimation.Progress, ringColor);

        Rectangle middle = Rectangle.Round(circle);
        int middleY = middle.Y + middle.Height / 2;
        DarkTheme.DrawText(graphics, valueText, DarkTheme.BigNumberFont,
            new Rectangle(middle.X, middleY - 30, middle.Width, 40), valueColor, TextFormatFlags.HorizontalCenter);
        DarkTheme.DrawText(graphics, subText, DarkTheme.NormalFont,
            new Rectangle(middle.X, middleY + 10, middle.Width, 20), DarkTheme.MutedTextColor, TextFormatFlags.HorizontalCenter);
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
