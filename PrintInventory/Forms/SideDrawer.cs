namespace PrintInventory.Forms;

// A panel that slides in from the right edge of the window, over a darkened background.
// Any content can be shown inside it (spool details now, other screens later).
public class SideDrawer : Panel
{
    // Use the WinForms timer: its Tick runs on the UI thread, so it may move controls.
    private readonly System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer { Interval = 15 };
    private readonly DimOverlay overlay = new DimOverlay();

    private Form? hostForm;
    private Control? content;
    private int targetLeft;
    private bool isClosing;

    public bool IsOpen => hostForm != null;

    public SideDrawer()
    {
        BackColor = DarkTheme.CardColor;
        Padding = new Padding(26, 20, 26, 20);
        DoubleBuffered = true;
        animationTimer.Tick += AnimationTimer_Tick;
        overlay.Click += Overlay_Click;
    }

    // contentToCapture: the window content that is shown darkened behind the drawer.
    // width: each content can have its own width (the manage drawer is wider, for the gallery).
    public void Open(Form host, Control contentToCapture, Control newContent, int width)
    {
        if (IsOpen)
        {
            CloseImmediately();
        }
        Width = width;

        hostForm = host;
        content = newContent;
        content.Dock = DockStyle.Fill;
        Controls.Add(content);

        overlay.ShowOver(host, contentToCapture);

        // Start just outside the right edge, then slide in.
        Top = 0;
        Height = host.ClientSize.Height;
        Left = host.ClientSize.Width;
        host.Controls.Add(this);
        BringToFront();
        host.Resize += Host_Resize;

        targetLeft = host.ClientSize.Width - Width;
        isClosing = false;
        animationTimer.Start();
    }

    public void Close()
    {
        if (hostForm == null)
        {
            return;
        }
        targetLeft = hostForm.ClientSize.Width; // slide out to the right
        isClosing = true;
        animationTimer.Start();
    }

    // Each tick moves the drawer one third of the remaining distance:
    // fast at first, slower near the end ("ease out").
    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        int distance = targetLeft - Left;
        if (Math.Abs(distance) <= 2)
        {
            Left = targetLeft;
            animationTimer.Stop();
            if (isClosing)
            {
                CloseImmediately();
            }
            return;
        }

        int step = distance / 3;
        if (step == 0)
        {
            step = Math.Sign(distance);
        }
        Left += step;
    }

    private void CloseImmediately()
    {
        animationTimer.Stop();
        if (hostForm != null)
        {
            hostForm.Resize -= Host_Resize;
            hostForm.Controls.Remove(this);
        }
        overlay.HideOverlay();

        if (content != null)
        {
            Controls.Remove(content);
            content.Dispose();
            content = null;
        }
        hostForm = null;
    }

    // Keep the drawer on the right edge when the window is resized.
    private void Host_Resize(object? sender, EventArgs e)
    {
        if (hostForm == null)
        {
            return;
        }
        Height = hostForm.ClientSize.Height;
        overlay.Bounds = hostForm.ClientRectangle;
        targetLeft = isClosing ? hostForm.ClientSize.Width : hostForm.ClientSize.Width - Width;
        Left = targetLeft;
    }

    private void Overlay_Click(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        // A thin glowing line on the left edge.
        using var glowPen = new Pen(Color.FromArgb(60, DarkTheme.AccentColor), 3);
        using var linePen = new Pen(DarkTheme.AccentColor, 1);
        e.Graphics.DrawLine(glowPen, 1, 0, 1, Height);
        e.Graphics.DrawLine(linePen, 0, 0, 0, Height);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            animationTimer.Dispose();
            overlay.Dispose();
        }
        base.Dispose(disposing);
    }
}
