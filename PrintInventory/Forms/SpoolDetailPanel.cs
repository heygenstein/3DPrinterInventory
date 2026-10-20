using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// The content of the drawer for one spool: big ring, facts, actions and its print history.
public class SpoolDetailPanel : Panel
{
    private readonly int spoolId;
    private readonly SpoolService spoolService = new SpoolService();
    private readonly PrintJobService printJobService = new PrintJobService();

    private readonly DrawerHeader header = new DrawerHeader();
    private readonly Label subtitleLabel = new Label
    {
        AutoSize = true,
        Font = DarkTheme.SmallFont,
        ForeColor = DarkTheme.MutedTextColor,
        Margin = new Padding(0, 0, 0, 4)
    };
    private readonly RingView ringView = new RingView();
    private readonly InfoList infoList = new InfoList();
    private readonly Label historyLabel = CreateSectionLabel("");
    private readonly RecentPrintsList historyList = new RecentPrintsList();
    private readonly NeonButton archiveButton = new NeonButton("Arşivle", isPrimary: false);

    private bool isArchived;

    // Raised after something was saved, so the main screen can refresh.
    public event EventHandler? DataChanged;
    // Raised when the drawer should close (close button, or the spool was archived).
    public event EventHandler? CloseRequested;

    public SpoolDetailPanel(int spoolId)
    {
        this.spoolId = spoolId;
        BackColor = DarkTheme.CardColor;
        BuildLayout();
        LoadSpool();
    }

    private static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = DarkTheme.SectionFont,
            ForeColor = DarkTheme.MutedTextColor,
            Margin = new Padding(0, 14, 0, 8)
        };
    }

    private void BuildLayout()
    {
        header.CloseClicked += Header_CloseClicked;

        // Action buttons
        var adjustButton = new NeonButton("Kalanı düzelt", isPrimary: true) { Margin = new Padding(0, 0, 10, 0) };
        adjustButton.Click += AdjustButton_Click;
        var editButton = new NeonButton("Düzenle", isPrimary: false) { Margin = new Padding(0, 0, 10, 0) };
        editButton.Click += EditButton_Click;
        archiveButton.Margin = new Padding(0);
        archiveButton.Click += ArchiveButton_Click;
        var buttonsPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 16, 0, 0) };
        buttonsPanel.Controls.Add(adjustButton);
        buttonsPanel.Controls.Add(editButton);
        buttonsPanel.Controls.Add(archiveButton);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = DarkTheme.CardColor };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));                       // header
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));                       // subtitle
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));                  // ring
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8 * InfoList.RowHeight)); // facts (8 rows)
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));                       // buttons
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));                       // history title
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));                   // history list
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(subtitleLabel, 0, 1);
        layout.Controls.Add(ringView, 0, 2);
        layout.Controls.Add(infoList, 0, 3);
        layout.Controls.Add(buttonsPanel, 0, 4);
        layout.Controls.Add(historyLabel, 0, 5);
        layout.Controls.Add(historyList, 0, 6);

        Controls.Add(layout);
    }

    private void LoadSpool()
    {
        Spool? spool;
        List<PrintJob> jobs;
        using (var db = new AppDbContext())
        {
            spool = db.Spools.Include(s => s.Filament).FirstOrDefault(s => s.Id == spoolId);
            jobs = db.PrintJobs
                .Include(j => j.PrintModel)
                .Include(j => j.Spool)
                    .ThenInclude(s => s.Filament)
                .Where(j => j.SpoolId == spoolId)
                .OrderByDescending(j => j.PrintDate)
                .ThenByDescending(j => j.Id)
                .ToList();
        }
        if (spool == null)
        {
            throw new InvalidOperationException($"Spool with Id {spoolId} was not found.");
        }

        double usedGrams = spoolService.GetUsedGrams(spoolId);
        double remainingGrams = spoolService.CalculateRemainingGrams(spool, usedGrams);
        double remainingPercent = spoolService.CalculateRemainingPercent(spool, remainingGrams);
        bool isLowStock = spoolService.IsLowStock(spool, remainingGrams);
        isArchived = spool.IsArchived;

        header.Title = $"#{spool.Id}  {spool.Filament.Brand}";
        subtitleLabel.Text = $"{DisplayText.ForMaterial(spool.Filament.Material)} · {spool.Filament.ColorName}" + (isArchived ? "  ·  ARŞİVDE" : "");

        Color ringColor = DarkTheme.GetVisibleColor(HexColor.ToColor(spool.Filament.ColorHex));
        Color valueColor = isLowStock ? DarkTheme.WarningColor : DarkTheme.TextColor;
        string subText = isLowStock
            ? $"{DisplayText.Percent(remainingPercent)}  ·  AZ STOK"
            : $"{DisplayText.Percent(remainingPercent)} kaldı";
        ringView.SetValue(remainingPercent, ringColor, $"{remainingGrams:0} g", subText, valueColor);

        decimal pricePerGram = printJobService.CalculateCost(spool, 1);
        string adjustmentText = spool.AdjustmentGrams == 0 ? "yok" : $"{spool.AdjustmentGrams:+0.#;-0.#} g"; // always show the sign
        infoList.SetRows(new List<InfoRow>
        {
            new InfoRow("Başlangıç ağırlığı", $"{spool.InitialWeightGrams:0} g"),
            new InfoRow("Baskılarda kullanılan", $"{usedGrams:0.#} g"),
            new InfoRow("Manuel düzeltme", adjustmentText),
            new InfoRow("Kalan", $"{remainingGrams:0.#} g", valueColor),
            new InfoRow("Az stok uyarısı", $"{spool.LowStockThresholdGrams:0} g"),
            new InfoRow("Ödenen fiyat", $"{spool.PricePaid:0.00}"),
            new InfoRow("Gram başı fiyat", $"{pricePerGram:0.000}"),
            new InfoRow("Alış tarihi", spool.PurchaseDate.ToShortDateString())
        });

        archiveButton.Text = isArchived ? "Arşivden çıkar" : "Arşivle";

        historyLabel.Text = $"BU MAKARADAKİ BASKILAR ({jobs.Count})";
        var historyItems = new List<RecentPrintItem>();
        foreach (PrintJob job in jobs)
        {
            historyItems.Add(RecentPrintItem.FromJob(job, printJobService.CalculateCost(job.Spool, job.UsedGrams)));
        }
        historyList.SetItems(historyItems);
    }

    private void Header_CloseClicked(object? sender, EventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void AdjustButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new AdjustRemainingForm(spoolId);
        if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
        {
            LoadSpool();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EditButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new SpoolEditForm(spoolId);
        if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
        {
            LoadSpool();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ArchiveButton_Click(object? sender, EventArgs e)
    {
        string question = isArchived
            ? "Bu makara tekrar aktif makaralara alınsın mı?"
            : "Bu makara arşivlensin mi? Raftan kalkar ama baskı geçmişi korunur.";
        if (!DarkMessageBox.AskYesNo(FindForm(), question, "Makarayı arşivle"))
        {
            return;
        }

        spoolService.SetArchived(spoolId, !isArchived);
        DataChanged?.Invoke(this, EventArgs.Empty);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
