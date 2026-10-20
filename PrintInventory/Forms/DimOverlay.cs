namespace PrintInventory.Forms;

// A darkened picture of the window, shown behind an open drawer.
//
// WinForms cannot draw a half-transparent control on top of other controls.
// So we "fake" it: take a picture of the window (DrawToBitmap), darken the picture,
// and show the picture on top. Clicking it closes the drawer.
public class DimOverlay : DrawnControl
{
    private Bitmap? darkenedPicture;

    public void ShowOver(Form host, Control contentToCapture)
    {
        Size size = host.ClientSize;
        darkenedPicture?.Dispose();
        darkenedPicture = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));

        contentToCapture.DrawToBitmap(darkenedPicture, new Rectangle(Point.Empty, contentToCapture.Size));
        using (Graphics graphics = Graphics.FromImage(darkenedPicture))
        using (var darkBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0))) // 150 of 255 = about 60 % black
        {
            graphics.FillRectangle(darkBrush, 0, 0, size.Width, size.Height);
        }

        Bounds = host.ClientRectangle;
        host.Controls.Add(this);
        BringToFront();
    }

    public void HideOverlay()
    {
        Parent?.Controls.Remove(this);
        darkenedPicture?.Dispose();
        darkenedPicture = null;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (darkenedPicture == null)
        {
            e.Graphics.Clear(DarkTheme.WindowColor);
            return;
        }
        // If the window was resized, the picture is stretched to the new size.
        e.Graphics.DrawImage(darkenedPicture, ClientRectangle);
    }
}
