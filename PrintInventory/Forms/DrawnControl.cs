using System.Drawing.Drawing2D;

namespace PrintInventory.Forms;

// Base class for controls that draw everything themselves in OnPaint.
// It only switches on the settings every such control needs, so they are not repeated.
public class DrawnControl : Control
{
    public DrawnControl()
    {
        // UserPaint + AllPaintingInWmPaint: we do all the drawing ourselves.
        // OptimizedDoubleBuffer: draw into a hidden image first, then show it at once (no flicker).
        // ResizeRedraw: draw again when the size changes.
        SetStyle(ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);

        BackColor = DarkTheme.WindowColor;
        ForeColor = DarkTheme.TextColor;
        Font = DarkTheme.NormalFont;
    }

    // Smooth (anti-aliased) edges for circles and round corners.
    protected static void PrepareGraphics(Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
    }
}
