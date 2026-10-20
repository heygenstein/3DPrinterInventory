using PrintInventory.Data;

namespace PrintInventory.Forms;

// The data for one row in the recent prints list.
public class RecentPrintItem
{
    public string ModelName { get; set; } = "";
    public double UsedGrams { get; set; }
    public decimal Cost { get; set; }
    public DateTime PrintDate { get; set; }
    public PrintResult Result { get; set; }
    public string FilamentColorHex { get; set; } = "#000000";

    // The job must be loaded with its PrintModel and Spool.Filament (Include).
    public static RecentPrintItem FromJob(PrintJob job, decimal cost)
    {
        return new RecentPrintItem
        {
            ModelName = job.PrintModel.Name,
            UsedGrams = job.UsedGrams,
            Cost = cost,
            PrintDate = job.PrintDate,
            Result = job.Result,
            FilamentColorHex = job.Spool.Filament.ColorHex
        };
    }
}

// A drawn list of the last print jobs: result dot, model name, grams · cost · age, filament color.
public class RecentPrintsList : DrawnControl
{
    private const int RowHeight = 54;
    private List<RecentPrintItem> items = new List<RecentPrintItem>();

    public RecentPrintsList()
    {
        BackColor = DarkTheme.CardColor;
        Dock = DockStyle.Fill;
    }

    public void SetItems(List<RecentPrintItem> newItems)
    {
        items = newItems;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PrepareGraphics(graphics);
        graphics.Clear(BackColor);

        if (items.Count == 0)
        {
            DarkTheme.DrawText(graphics, "Henüz baskı yok. \"+ Baskı\" ile ekleyin.", DarkTheme.SmallFont,
                new Rectangle(0, 8, Width, 20), DarkTheme.MutedTextColor);
            return;
        }

        using var separatorPen = new Pen(DarkTheme.BorderColor);
        for (int i = 0; i < items.Count; i++)
        {
            int top = i * RowHeight;
            if (top + RowHeight > Height)
            {
                break; // no more room: show only the rows that fit
            }
            DrawRow(graphics, items[i], top);

            if (i < items.Count - 1)
            {
                graphics.DrawLine(separatorPen, 0, top + RowHeight - 1, Width, top + RowHeight - 1);
            }
        }
    }

    private void DrawRow(Graphics graphics, RecentPrintItem item, int top)
    {
        DarkTheme.DrawDot(graphics, new PointF(8, top + 17), 9, DarkTheme.GetResultColor(item.Result), withGlow: true);

        int textWidth = Width - 28 - 30;
        DarkTheme.DrawText(graphics, item.ModelName, DarkTheme.NormalBoldFont,
            new Rectangle(26, top + 7, textWidth, 20), DarkTheme.TextColor);

        string details = $"{item.UsedGrams:0.#} g  ·  {item.Cost:0.00}  ·  {DisplayText.FormatAge(item.PrintDate)}";
        if (item.Result != PrintResult.Success)
        {
            details = $"{DisplayText.ForResult(item.Result)}  ·  " + details;
        }
        DarkTheme.DrawText(graphics, details, DarkTheme.SmallFont,
            new Rectangle(26, top + 29, textWidth, 16), DarkTheme.MutedTextColor);

        // filament color on the right
        var swatchCenter = new PointF(Width - 12, top + 22);
        DarkTheme.DrawDot(graphics, swatchCenter, 14, HexColor.ToColor(item.FilamentColorHex), withGlow: false);
        using var outlinePen = new Pen(DarkTheme.MutedTextColor); // visible even for black filament
        graphics.DrawEllipse(outlinePen, swatchCenter.X - 7, swatchCenter.Y - 7, 14, 14);
    }

}
