using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using PrintInventory.Data;

namespace PrintInventory.Forms;

// The dark "neon" look of the new main screen: colors, fonts and drawing helpers.
// (UiTheme is still used by the older screens until they are redesigned too.)
public static class DarkTheme
{
    public static readonly Color WindowColor = Color.FromArgb(13, 15, 20);     // almost black
    public static readonly Color CardColor = Color.FromArgb(23, 26, 33);       // dark gray cards
    public static readonly Color CardHoverColor = Color.FromArgb(31, 35, 45);
    public static readonly Color BorderColor = Color.FromArgb(42, 47, 59);
    public static readonly Color TrackColor = Color.FromArgb(38, 43, 54);      // the empty part of a ring
    public static readonly Color InputColor = Color.FromArgb(14, 17, 23);      // text boxes in dialogs
    public static readonly Color TextColor = Color.FromArgb(232, 236, 242);
    public static readonly Color MutedTextColor = Color.FromArgb(128, 138, 155);
    public static readonly Color AccentColor = Color.FromArgb(0, 212, 255);    // neon cyan
    public static readonly Color WarningColor = Color.FromArgb(255, 77, 94);   // low stock, failed print
    public static readonly Color SuccessColor = Color.FromArgb(46, 230, 140);

    public static readonly Font TitleFont = CreateFont(13F, FontStyle.Bold);
    public static readonly Font BigNumberFont = CreateFont(24F, FontStyle.Bold);
    public static readonly Font RingValueFont = CreateFont(15F, FontStyle.Bold);
    public static readonly Font NormalFont = CreateFont(10F);
    public static readonly Font NormalBoldFont = CreateFont(10F, FontStyle.Bold);
    public static readonly Font SmallFont = CreateFont(8.5F);
    public static readonly Font SectionFont = CreateFont(8.5F, FontStyle.Bold);

    // Windows 11 has the modern "Segoe UI Variable" font. On older Windows we use "Segoe UI".
    // (If a font is not installed, WinForms silently uses another one; we check the name.)
    private static Font CreateFont(float size, FontStyle style = FontStyle.Regular)
    {
        var font = new Font("Segoe UI Variable Display", size, style);
        if (font.Name != "Segoe UI Variable Display")
        {
            font.Dispose();
            font = new Font("Segoe UI", size, style);
        }
        return font;
    }

    // A rectangle with round corners, as a path that can be filled or outlined.
    public static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
    {
        float diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);                                   // top left
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);                    // top right
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);      // bottom right
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);                    // bottom left
        path.CloseFigure();
        return path;
    }

    public static void DrawCard(Graphics graphics, RectangleF bounds, Color fillColor, Color borderColor, float radius = 12)
    {
        using GraphicsPath path = CreateRoundedRectangle(bounds, radius);
        using var brush = new SolidBrush(fillColor);
        using var pen = new Pen(borderColor, 1);
        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
    }

    // A soft "neon" glow around a card: the outline drawn a few times,
    // wide and transparent first, then thinner and stronger.
    public static void DrawGlow(Graphics graphics, RectangleF bounds, Color glowColor, float radius = 12)
    {
        using GraphicsPath path = CreateRoundedRectangle(bounds, radius);
        int[] widths = { 8, 5, 2 };
        int[] alphas = { 25, 45, 90 };
        for (int i = 0; i < widths.Length; i++)
        {
            using var pen = new Pen(Color.FromArgb(alphas[i], glowColor), widths[i]);
            graphics.DrawPath(pen, path);
        }
    }

    // Draws a ring (donut) that is "percent" full, starting at the top and going clockwise.
    public static void DrawRing(Graphics graphics, RectangleF circle, float thickness, double percent, Color color)
    {
        // A pen draws on the middle of the line, so make the circle smaller by half the
        // line width (plus room for the glow), otherwise the ring is cut off at the edges.
        float inset = thickness / 2 + 3;
        RectangleF ring = RectangleF.Inflate(circle, -inset, -inset);

        using (var trackPen = new Pen(TrackColor, thickness))
        {
            graphics.DrawEllipse(trackPen, ring);
        }

        if (percent <= 0)
        {
            return;
        }
        float sweepAngle = (float)(360 * Math.Min(percent, 100) / 100);

        // Glow: a wider, transparent arc behind the real one.
        using (var glowPen = new Pen(Color.FromArgb(55, color), thickness + 6))
        {
            glowPen.StartCap = LineCap.Round;
            glowPen.EndCap = LineCap.Round;
            graphics.DrawArc(glowPen, ring, -90, sweepAngle); // -90 degrees = 12 o'clock
        }
        using (var pen = new Pen(color, thickness))
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            graphics.DrawArc(pen, ring, -90, sweepAngle);
        }
    }

    public static void DrawDot(Graphics graphics, PointF center, float diameter, Color color, bool withGlow)
    {
        if (withGlow)
        {
            float glowSize = diameter * 2;
            using var glowBrush = new SolidBrush(Color.FromArgb(60, color));
            graphics.FillEllipse(glowBrush, center.X - glowSize / 2, center.Y - glowSize / 2, glowSize, glowSize);
        }
        using var brush = new SolidBrush(color);
        graphics.FillEllipse(brush, center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
    }

    // TextRenderer draws sharper text than Graphics.DrawString.
    public static void DrawText(Graphics graphics, string text, Font font, Rectangle bounds, Color color,
        TextFormatFlags alignment = TextFormatFlags.Left | TextFormatFlags.Top)
    {
        TextFormatFlags flags = alignment | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(graphics, text, font, bounds, color, flags);
    }

    // Very dark filament colors (black) would be invisible on the dark background.
    // For drawing rings we mix them with some white. Brightness formula: standard luminance.
    public static Color GetVisibleColor(Color color)
    {
        double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        if (brightness >= 0.25)
        {
            return color;
        }
        return Blend(color, Color.White, 0.35);
    }

    // Mixes two colors. amount 0 = only "from", 1 = only "to".
    public static Color Blend(Color from, Color to, double amount)
    {
        int red = (int)(from.R + (to.R - from.R) * amount);
        int green = (int)(from.G + (to.G - from.G) * amount);
        int blue = (int)(from.B + (to.B - from.B) * amount);
        return Color.FromArgb(red, green, blue);
    }

    // Success = green, Failed = red, Cancelled = gray.
    public static Color GetResultColor(PrintResult result)
    {
        switch (result)
        {
            case PrintResult.Success: return SuccessColor;
            case PrintResult.Failed: return WarningColor;
            default: return MutedTextColor;
        }
    }

    // One fixed color per material, used in the material donut chart.
    public static Color GetMaterialColor(FilamentMaterial material)
    {
        switch (material)
        {
            case FilamentMaterial.PLA: return AccentColor;
            case FilamentMaterial.PETG: return Color.FromArgb(166, 107, 255); // purple
            case FilamentMaterial.ABS: return Color.FromArgb(255, 159, 67);   // orange
            case FilamentMaterial.TPU: return SuccessColor;                   // green
            case FilamentMaterial.ASA: return Color.FromArgb(255, 217, 61);   // yellow
            default: return MutedTextColor;
        }
    }

    // --- App icon (title bar and taskbar) ---
    // Drawn in code: a dark circle with a neon ring, so we need no .ico file.
    private static Icon? appIcon;

    public static Icon AppIcon
    {
        get
        {
            if (appIcon == null)
            {
                appIcon = CreateAppIcon(); // made only once, then reused
            }
            return appIcon;
        }
    }

    private static Icon CreateAppIcon()
    {
        using var bitmap = new Bitmap(64, 64);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            using (var backgroundBrush = new SolidBrush(WindowColor))
            {
                graphics.FillEllipse(backgroundBrush, 1, 1, 62, 62);
            }
            DrawRing(graphics, new RectangleF(4, 4, 56, 56), 9, 75, AccentColor);
        }
        // GetHicon turns the picture into a Windows icon. The icon lives as long as the app,
        // so we do not free its handle.
        return Icon.FromHandle(bitmap.GetHicon());
    }

    // --- Dark scroll bars ---
    // Windows has a built-in dark style ("DarkMode_Explorer") for standard scroll bars.
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr windowHandle, string? appName, string? idList);

    public static void ApplyDarkScrollBars(Control control)
    {
        ApplyWindowTheme(control, "DarkMode_Explorer");
    }

    // "DarkMode_CFD" is the dark style Windows uses in its own file dialogs; it also works for combo boxes.
    public static void ApplyDarkComboBox(ComboBox comboBox)
    {
        ApplyWindowTheme(comboBox, "DarkMode_CFD");
    }

    // A theme can only be set on a control that Windows has already created (it needs a Handle).
    // If it does not exist yet, we wait for the HandleCreated event.
    private static void ApplyWindowTheme(Control control, string themeName)
    {
        if (control.IsHandleCreated)
        {
            SetWindowTheme(control.Handle, themeName, null);
        }
        else
        {
            control.HandleCreated += (sender, e) => SetWindowTheme(control.Handle, themeName, null);
        }
    }

    // --- Dark window title bar (Windows 10 20H1+ / Windows 11) ---
    // WinForms has no setting for this, so we ask Windows directly through its
    // Desktop Window Manager (dwmapi.dll). [DllImport] lets C# call a Windows function.
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int value, int valueSize);

    private const int DwmUseImmersiveDarkMode = 20;
    private const int DwmCaptionColor = 35; // Windows 11 only; older Windows ignores it

    public static void ApplyDarkTitleBar(Form form)
    {
        int enabled = 1;
        DwmSetWindowAttribute(form.Handle, DwmUseImmersiveDarkMode, ref enabled, sizeof(int));

        // Windows wants the color as 0x00BBGGRR (blue, green, red).
        int captionColor = WindowColor.R | (WindowColor.G << 8) | (WindowColor.B << 16);
        DwmSetWindowAttribute(form.Handle, DwmCaptionColor, ref captionColor, sizeof(int));
    }
}
