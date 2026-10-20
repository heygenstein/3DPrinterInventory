using PrintInventory.Services;

namespace PrintInventory.Forms;

// A donut chart: how the filament in stock is split between materials (PLA, PETG, ...).
// Each material gets a slice of the ring; a legend with percentages is on the right.
public class MaterialDonut : DrawnControl
{
    private const float Thickness = 16;
    private const float GapAngle = 3; // small gap between slices

    private List<MaterialShare> shares = new List<MaterialShare>();

    public MaterialDonut()
    {
        BackColor = DarkTheme.CardColor;
        Dock = DockStyle.Fill;
    }

    public void SetShares(List<MaterialShare> newShares)
    {
        shares = newShares;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        double totalGrams = shares.Sum(s => s.RemainingGrams);
        if (totalGrams <= 0)
        {
            DarkTheme.DrawText(graphics, "Stokta filament yok.", DarkTheme.SmallFont,
                new Rectangle(0, 8, Width, 20), DarkTheme.MutedTextColor);
            return;
        }

        int size = Math.Min(Height - 8, 136);
        var circle = new RectangleF(2, (Height - size) / 2f, size, size);
        DrawSlices(graphics, circle, totalGrams);

        // total in the middle
        Rectangle middle = Rectangle.Round(circle);
        int middleY = middle.Y + middle.Height / 2;
        DarkTheme.DrawText(graphics, $"{totalGrams / 1000:0.0}", DarkTheme.RingValueFont,
            new Rectangle(middle.X, middleY - 20, middle.Width, 26), DarkTheme.TextColor, TextFormatFlags.HorizontalCenter);
        DarkTheme.DrawText(graphics, "kg", DarkTheme.SmallFont,
            new Rectangle(middle.X, middleY + 6, middle.Width, 16), DarkTheme.MutedTextColor, TextFormatFlags.HorizontalCenter);

        DrawLegend(graphics, (int)circle.Right + 22, totalGrams);
    }

    private void DrawSlices(Graphics graphics, RectangleF circle, double totalGrams)
    {
        float inset = Thickness / 2 + 2;
        RectangleF ring = RectangleF.Inflate(circle, -inset, -inset);

        using (var trackPen = new Pen(DarkTheme.TrackColor, Thickness))
        {
            graphics.DrawEllipse(trackPen, ring);
        }

        float gap = shares.Count > 1 ? GapAngle : 0;
        float startAngle = -90; // 12 o'clock
        foreach (MaterialShare share in shares)
        {
            float sweepAngle = (float)(360 * share.RemainingGrams / totalGrams);
            using var pen = new Pen(DarkTheme.GetMaterialColor(share.Material), Thickness);
            graphics.DrawArc(pen, ring, startAngle + gap / 2, Math.Max(0.5f, sweepAngle - gap));
            startAngle += sweepAngle;
        }
    }

    private void DrawLegend(Graphics graphics, int left, double totalGrams)
    {
        const int rowHeight = 24;
        int maxRows = Math.Max(1, Height / rowHeight);
        int rowCount = Math.Min(shares.Count, maxRows);
        int top = (Height - rowCount * rowHeight) / 2;

        for (int i = 0; i < rowCount; i++)
        {
            MaterialShare share = shares[i];
            int rowTop = top + i * rowHeight;
            double percent = 100 * share.RemainingGrams / totalGrams;

            DarkTheme.DrawDot(graphics, new PointF(left + 5, rowTop + 10), 10, DarkTheme.GetMaterialColor(share.Material), withGlow: true);
            DarkTheme.DrawText(graphics, DisplayText.ForMaterial(share.Material), DarkTheme.NormalFont,
                new Rectangle(left + 18, rowTop + 1, 70, 20), DarkTheme.TextColor);
            DarkTheme.DrawText(graphics, DisplayText.Percent(percent), DarkTheme.SmallFont,
                new Rectangle(left + 18, rowTop + 3, Width - left - 22, 20), DarkTheme.MutedTextColor, TextFormatFlags.Right);
        }
    }
}
