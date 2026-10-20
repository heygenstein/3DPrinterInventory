using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// The main window: one dark, single-page overview.
//
//  [ header bar: logo · printer chip ·                 + Print  + Spool  Manage ]
//  [ tile ][ tile ][ tile ][ tile ][ tile with ring ]
//  [ SPOOL RACK: one ring card per spool          ][ RECENT PRINTS          ]
//  [                                              ][ MATERIALS donut chart  ]
public class MainForm : Form
{
    private const int RecentPrintsCount = 8;

    private readonly DashboardService dashboardService = new DashboardService();
    private readonly PrintJobService printJobService = new PrintJobService();

    private readonly HeaderBar headerBar = new HeaderBar { Dock = DockStyle.Fill, Margin = new Padding(0) };
    private readonly StatTile filamentLeftTile = new StatTile("KALAN FİLAMENT");
    private readonly StatTile activeSpoolsTile = new StatTile("AKTİF MAKARA");
    private readonly StatTile lowStockTile = new StatTile("AZ STOK");
    private readonly StatTile printsTile = new StatTile("BASKI");
    private readonly StatTile successTile = new StatTile("BAŞARI ORANI");
    private readonly FlowLayoutPanel spoolRack = new FlowLayoutPanel
    {
        Dock = DockStyle.Fill,
        AutoScroll = true, // a scroll bar appears when there are more cards than fit
        BackColor = DarkTheme.WindowColor,
        Margin = new Padding(0),
        Padding = new Padding(0)
    };
    private readonly RecentPrintsList recentPrintsList = new RecentPrintsList();
    private readonly MaterialDonut materialDonut = new MaterialDonut();
    private readonly SideDrawer drawer = new SideDrawer();
    private TableLayoutPanel root = null!; // the whole page; created in BuildLayout
    private bool isFirstLoad = true;      // the rings fill up only when the app starts

    public MainForm()
    {
        Text = "Baskı Envanteri";
        Icon = DarkTheme.AppIcon;
        BackColor = DarkTheme.WindowColor;
        ForeColor = DarkTheme.TextColor;
        Font = DarkTheme.NormalFont;
        Size = new Size(1400, 880);
        MinimumSize = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        LoadData();
    }

    // Esc closes an open drawer.
    // ProcessCmdKey runs before any control handles the key, no matter which control has focus.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape && drawer.IsOpen)
        {
            drawer.Close();
            return true; // "handled", nobody else gets this key
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            drawer.Dispose(); // it is not always inside Controls, so free it ourselves
        }
        base.Dispose(disposing);
    }

    // The window handle exists only after the window is created; then we can make the title bar dark.
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        DarkTheme.ApplyDarkTitleBar(this);
        DarkTheme.ApplyDarkScrollBars(spoolRack);
    }

    private void BuildLayout()
    {
        NeonButton addPrintButton = new NeonButton("+ Baskı", isPrimary: true);
        addPrintButton.Click += AddPrintButton_Click;
        NeonButton addSpoolButton = new NeonButton("+ Makara", isPrimary: false);
        addSpoolButton.Click += AddSpoolButton_Click;
        NeonButton manageButton = new NeonButton("☰  Yönet", isPrimary: false);
        manageButton.Click += ManageButton_Click;
        headerBar.ButtonsPanel.Controls.Add(addPrintButton);
        headerBar.ButtonsPanel.Controls.Add(addSpoolButton);
        headerBar.ButtonsPanel.Controls.Add(manageButton);

        root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = DarkTheme.WindowColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));   // header
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));  // tiles
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // spool rack + sidebar
        root.Controls.Add(headerBar, 0, 0);
        root.Controls.Add(BuildTilesRow(), 0, 1);
        root.Controls.Add(BuildContentRow(), 0, 2);

        Controls.Add(root);
    }

    private TableLayoutPanel BuildTilesRow()
    {
        var tilesRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(24, 20, 10, 0),
            BackColor = DarkTheme.WindowColor
        };
        StatTile[] tiles = { filamentLeftTile, activeSpoolsTile, lowStockTile, printsTile, successTile };
        for (int i = 0; i < tiles.Length; i++)
        {
            tilesRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // 5 equal columns
            tilesRow.Controls.Add(tiles[i], i, 0);
        }
        return tilesRow;
    }

    private TableLayoutPanel BuildContentRow()
    {
        // Left: spool rack. Right: a card with recent prints and the material chart.
        var spoolArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 14, 0),
            BackColor = DarkTheme.WindowColor
        };
        spoolArea.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        spoolArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        spoolArea.Controls.Add(CreateSectionLabel("MAKARA RAFI", DarkTheme.WindowColor), 0, 0);
        spoolArea.Controls.Add(spoolRack, 0, 1);

        var sidebarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DarkTheme.CardColor,
            Margin = new Padding(0)
        };
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        sidebarLayout.Controls.Add(CreateSectionLabel("SON BASKILAR", DarkTheme.CardColor), 0, 0);
        sidebarLayout.Controls.Add(recentPrintsList, 0, 1);
        sidebarLayout.Controls.Add(CreateSectionLabel("STOKTAKİ MALZEMELER", DarkTheme.CardColor), 0, 2);
        sidebarLayout.Controls.Add(materialDonut, 0, 3);

        var sidebar = new CardPanel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        sidebar.Controls.Add(sidebarLayout);

        var contentRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(24, 22, 24, 24),
            BackColor = DarkTheme.WindowColor
        };
        contentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370));
        contentRow.Controls.Add(spoolArea, 0, 0);
        contentRow.Controls.Add(sidebar, 1, 0);
        return contentRow;
    }

    private static Label CreateSectionLabel(string text, Color backColor)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = DarkTheme.SectionFont,
            ForeColor = DarkTheme.MutedTextColor,
            BackColor = backColor,
            Margin = new Padding(2, 0, 0, 12)
        };
    }

    // Reads everything from the database and shows it. Called at start and after every change.
    private void LoadData()
    {
        List<SpoolStatus> spools = dashboardService.GetActiveSpools();
        ShowTiles(spools);
        ShowSpoolRack(spools, animate: isFirstLoad);
        isFirstLoad = false;
        recentPrintsList.SetItems(LoadRecentPrints());
        materialDonut.SetShares(dashboardService.GetRemainingByMaterial(spools));
        headerBar.SetPrinterText(GetPrinterText());
    }

    private void ShowTiles(List<SpoolStatus> spools)
    {
        double totalGrams = dashboardService.GetTotalRemainingGrams(spools);
        filamentLeftTile.SetValue($"{totalGrams / 1000:0.0} kg", "tüm aktif makaralarda", DarkTheme.AccentColor);

        int archivedCount = dashboardService.GetArchivedSpoolCount();
        activeSpoolsTile.SetValue($"{spools.Count}", $"{archivedCount} arşivde", DarkTheme.AccentColor);

        int lowStockCount = spools.Count(s => s.IsLowStock);
        if (lowStockCount == 0)
        {
            lowStockTile.SetValue("0", "tüm makaralar yeterli", DarkTheme.SuccessColor);
        }
        else
        {
            lowStockTile.SetValue($"{lowStockCount}", "filament sipariş zamanı", DarkTheme.WarningColor);
        }

        double printedKg = dashboardService.GetTotalPrintedGrams() / 1000;
        printsTile.SetValue($"{dashboardService.GetPrintJobCount()}", $"{printedKg:0.0} kg basıldı", DarkTheme.AccentColor);

        double? successRate = dashboardService.GetSuccessRatePercent();
        if (successRate == null)
        {
            successTile.SetValue("-", "henüz tamamlanan baskı yok", DarkTheme.SuccessColor, 0);
        }
        else
        {
            successTile.SetValue(DisplayText.Percent(successRate.Value), "tamamlanan baskılarda", DarkTheme.SuccessColor, successRate);
        }
    }

    private void ShowSpoolRack(List<SpoolStatus> spools, bool animate)
    {
        // Remove and free the old cards before adding new ones.
        while (spoolRack.Controls.Count > 0)
        {
            Control oldCard = spoolRack.Controls[0];
            spoolRack.Controls.RemoveAt(0);
            oldCard.Dispose();
        }

        if (spools.Count == 0)
        {
            spoolRack.Controls.Add(new Label
            {
                Text = "Aktif makara yok. \"+ Makara\" ile ekleyin.",
                AutoSize = true,
                ForeColor = DarkTheme.MutedTextColor,
                Font = DarkTheme.NormalFont
            });
            return;
        }

        foreach (SpoolStatus spool in spools)
        {
            var card = new SpoolCard { Cursor = Cursors.Hand };
            card.SetStatus(spool, animate);
            card.Click += SpoolCard_Click;
            spoolRack.Controls.Add(card);
        }
    }

    private void SpoolCard_Click(object? sender, EventArgs e)
    {
        if (sender is SpoolCard card && card.Status != null)
        {
            OpenSpoolDetails(card.Status.Spool.Id);
        }
    }

    private void OpenSpoolDetails(int spoolId)
    {
        var detailPanel = new SpoolDetailPanel(spoolId);
        detailPanel.DataChanged += DetailPanel_DataChanged;
        detailPanel.CloseRequested += DetailPanel_CloseRequested;
        drawer.Open(this, root, detailPanel, 460);
    }

    private void DetailPanel_DataChanged(object? sender, EventArgs e)
    {
        LoadData(); // the page behind the drawer shows the new values when the drawer closes
    }

    private void DetailPanel_CloseRequested(object? sender, EventArgs e)
    {
        drawer.Close();
    }

    private List<RecentPrintItem> LoadRecentPrints()
    {
        List<PrintJob> jobs;
        using (var db = new AppDbContext())
        {
            jobs = db.PrintJobs
                .Include(j => j.PrintModel)
                .Include(j => j.Spool)
                    .ThenInclude(s => s.Filament)
                .OrderByDescending(j => j.PrintDate)
                .ThenByDescending(j => j.Id)
                .Take(RecentPrintsCount)
                .ToList();
        }

        var items = new List<RecentPrintItem>();
        foreach (PrintJob job in jobs)
        {
            items.Add(RecentPrintItem.FromJob(job, printJobService.CalculateCost(job.Spool, job.UsedGrams)));
        }
        return items;
    }

    // "Ender 3 S1 · 0.4 mm", or "Ender 3 S1 · 0.4 mm  +1" when there are more printers.
    private static string GetPrinterText()
    {
        using var db = new AppDbContext();
        List<Printer> printers = db.Printers.OrderBy(p => p.Id).ToList();
        if (printers.Count == 0)
        {
            return "";
        }

        Printer first = printers[0];
        string text = $"{first.Name} · {first.NozzleDiameterMm} mm";
        if (printers.Count > 1)
        {
            text += $"  +{printers.Count - 1}";
        }
        return text;
    }

    private void AddPrintButton_Click(object? sender, EventArgs e)
    {
        string? missingData = printJobService.FindMissingDataForNewJob();
        if (missingData != null)
        {
            DarkMessageBox.ShowInfo(this, $"Önce {missingData} ekleyin (☰ Yönet).", "Baskı ekle");
            return;
        }

        using var dialog = new PrintJobEditForm(null);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void AddSpoolButton_Click(object? sender, EventArgs e)
    {
        using (var db = new AppDbContext())
        {
            if (!db.Filaments.Any())
            {
                DarkMessageBox.ShowInfo(this, "Önce bir filament ekleyin (☰ Yönet → Filamentler).", "Makara ekle");
                return;
            }
        }

        using var dialog = new SpoolEditForm(null);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void ManageButton_Click(object? sender, EventArgs e)
    {
        var managePanel = new ManagePanel();
        managePanel.DataChanged += DetailPanel_DataChanged;
        managePanel.CloseRequested += DetailPanel_CloseRequested;
        drawer.Open(this, root, managePanel, 700);
    }
}
