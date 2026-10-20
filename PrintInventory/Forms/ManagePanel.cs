using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// The content of the "☰ Yönet" drawer: one place to manage filaments, models, printers,
// the full print history and the archived spools. The tabs at the top switch the section.
public class ManagePanel : Panel
{
    private const int FilamentsTab = 0;
    private const int ModelsTab = 1;
    private const int PrintersTab = 2;
    private const int HistoryTab = 3;
    private const int ArchiveTab = 4;

    private readonly SpoolService spoolService = new SpoolService();
    private readonly PrintJobService printJobService = new PrintJobService();

    private readonly DrawerHeader header = new DrawerHeader { Title = "Yönetim" };
    private readonly SegmentedTabs tabs = new SegmentedTabs("Filamentler", "Modeller", "Yazıcılar", "Geçmiş", "Arşiv");
    private readonly Label summaryLabel = new Label
    {
        AutoSize = true,
        Font = DarkTheme.SmallFont,
        ForeColor = DarkTheme.MutedTextColor,
        Anchor = AnchorStyles.Left
    };
    private readonly NeonButton addButton = new NeonButton("+ Ekle", isPrimary: true) { Margin = new Padding(0) };
    private readonly Panel contentHost = new Panel
    {
        Dock = DockStyle.Fill,
        BackColor = DarkTheme.CardColor,
        Margin = new Padding(0, 14, 0, 0)
    };

    // Raised after something was saved, so the main screen can refresh.
    public event EventHandler? DataChanged;
    public event EventHandler? CloseRequested;

    public ManagePanel()
    {
        BackColor = DarkTheme.CardColor;
        BuildLayout();
        ShowSelectedSection();
    }

    private void BuildLayout()
    {
        header.CloseClicked += Header_CloseClicked;
        tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;
        addButton.Click += AddButton_Click;

        // "12 filament" on the left, "+ Filament" on the right
        var actionBar = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        actionBar.Controls.Add(summaryLabel, 0, 0);
        actionBar.Controls.Add(addButton, 1, 0);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = DarkTheme.CardColor };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // header
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); // tabs (40) + margins
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // action bar
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // content
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(tabs, 0, 1);
        layout.Controls.Add(actionBar, 0, 2);
        layout.Controls.Add(contentHost, 0, 3);

        Controls.Add(layout);
    }

    private void Header_CloseClicked(object? sender, EventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Tabs_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ShowSelectedSection();
    }

    // Removes the old list and builds the list of the selected tab again from the database.
    private void ShowSelectedSection()
    {
        while (contentHost.Controls.Count > 0)
        {
            Control oldContent = contentHost.Controls[0];
            contentHost.Controls.RemoveAt(0);
            oldContent.Dispose(); // also frees the model pictures
        }

        addButton.Visible = true;
        Control content;
        switch (tabs.SelectedIndex)
        {
            case FilamentsTab:
                addButton.Text = "+ Filament";
                content = BuildFilamentsList();
                break;
            case ModelsTab:
                addButton.Text = "+ Model";
                content = BuildModelsGallery();
                break;
            case PrintersTab:
                addButton.Text = "+ Yazıcı";
                content = BuildPrintersList();
                break;
            case HistoryTab:
                addButton.Text = "+ Baskı";
                content = BuildHistoryList();
                break;
            default: // ArchiveTab
                addButton.Visible = false; // spools are archived from their detail drawer
                content = BuildArchiveList();
                break;
        }

        contentHost.Controls.Add(content);
        // The list was filled while it was still small (not yet docked). Now that it has its
        // real size, arrange the rows again, so no unneeded scroll bar stays visible.
        content.PerformLayout();
    }

    // ----- Filaments -----

    private RowListPanel BuildFilamentsList()
    {
        List<Filament> filaments;
        using (var db = new AppDbContext())
        {
            filaments = db.Filaments
                .Include(f => f.Spools)
                .OrderBy(f => f.Brand.ToLower())
                .ThenBy(f => f.ColorName.ToLower())
                .ToList();
        }
        summaryLabel.Text = $"{filaments.Count} filament";

        var list = new RowListPanel();
        if (filaments.Count == 0)
        {
            list.ShowEmptyText("Henüz filament yok. \"+ Filament\" ile ekleyin.");
        }
        foreach (Filament filament in filaments)
        {
            int activeSpoolCount = filament.Spools.Count(s => !s.IsArchived);
            var row = new ListRow
            {
                Title = $"{filament.Brand} {DisplayText.ForMaterial(filament.Material)}",
                Subtitle = $"{filament.ColorName}  ·  {filament.DiameterMm} mm  ·  {filament.DensityGPerCm3} g/cm³",
                RightText = $"{activeSpoolCount} makara",
                RightSubText = FormatTemperatures(filament),
                DotColor = HexColor.ToColor(filament.ColorHex),
                Tag = filament.Id
            };
            row.RowClicked += FilamentRow_Clicked;
            list.Controls.Add(row);
        }
        return list;
    }

    // "215 °C / 60 °C" (nozzle / bed), or "" when nothing is known.
    private static string FormatTemperatures(Filament filament)
    {
        if (filament.RecommendedNozzleTempC == null && filament.RecommendedBedTempC == null)
        {
            return "";
        }
        string nozzle = filament.RecommendedNozzleTempC?.ToString() ?? "-";
        string bed = filament.RecommendedBedTempC?.ToString() ?? "-";
        return $"{nozzle} °C / {bed} °C";
    }

    private void FilamentRow_Clicked(object? sender, EventArgs e)
    {
        if (sender is ListRow row)
        {
            ShowDialogAndRefresh(new FilamentEditForm((int)row.Tag!));
        }
    }

    // ----- Models -----

    private FlowLayoutPanel BuildModelsGallery()
    {
        List<PrintModel> models;
        using (var db = new AppDbContext())
        {
            models = db.PrintModels.OrderBy(m => m.Name.ToLower()).ToList();
        }
        summaryLabel.Text = $"{models.Count} model";

        var gallery = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = DarkTheme.CardColor,
            Margin = new Padding(0)
        };
        DarkTheme.ApplyDarkScrollBars(gallery);

        if (models.Count == 0)
        {
            gallery.Controls.Add(new Label
            {
                Text = "Henüz model yok. \"+ Model\" ile ekleyin.",
                AutoSize = true,
                ForeColor = DarkTheme.MutedTextColor
            });
        }
        foreach (PrintModel model in models)
        {
            var tile = new ModelTile { Tag = model.Id };
            tile.SetModel(model);
            tile.TileClicked += ModelTile_Clicked;
            gallery.Controls.Add(tile);
        }
        return gallery;
    }

    private void ModelTile_Clicked(object? sender, EventArgs e)
    {
        if (sender is ModelTile tile)
        {
            ShowDialogAndRefresh(new ModelEditForm((int)tile.Tag!));
        }
    }

    // ----- Printers -----

    private RowListPanel BuildPrintersList()
    {
        List<Printer> printers;
        using (var db = new AppDbContext())
        {
            printers = db.Printers.OrderBy(p => p.Name.ToLower()).ToList();
        }
        summaryLabel.Text = $"{printers.Count} yazıcı";

        var list = new RowListPanel();
        foreach (Printer printer in printers)
        {
            var row = new ListRow
            {
                Title = printer.Name,
                Subtitle = $"{printer.Brand}  ·  {printer.Model}",
                RightText = $"{printer.NozzleDiameterMm} mm nozül",
                RightSubText = $"{printer.BuildVolumeXMm} × {printer.BuildVolumeYMm} × {printer.BuildVolumeZMm} mm",
                DotColor = DarkTheme.AccentColor,
                DotGlow = true,
                Tag = printer.Id
            };
            row.RowClicked += PrinterRow_Clicked;
            list.Controls.Add(row);
        }
        return list;
    }

    private void PrinterRow_Clicked(object? sender, EventArgs e)
    {
        if (sender is ListRow row)
        {
            ShowDialogAndRefresh(new PrinterEditForm((int)row.Tag!));
        }
    }

    // ----- Print history -----

    private RowListPanel BuildHistoryList()
    {
        List<PrintJob> jobs;
        using (var db = new AppDbContext())
        {
            jobs = db.PrintJobs
                .Include(j => j.PrintModel)
                .Include(j => j.Printer)
                .Include(j => j.Spool)
                    .ThenInclude(s => s.Filament)
                .OrderByDescending(j => j.PrintDate)
                .ThenByDescending(j => j.Id)
                .ToList();
        }

        decimal totalCost = 0;
        var list = new RowListPanel();
        if (jobs.Count == 0)
        {
            list.ShowEmptyText("Henüz baskı yok. \"+ Baskı\" ile ekleyin.");
        }
        foreach (PrintJob job in jobs)
        {
            decimal cost = printJobService.CalculateCost(job.Spool, job.UsedGrams);
            totalCost += cost;

            string subtitle = $"{job.PrintDate:g}  ·  #{job.Spool.Id} {DisplayText.ForFilament(job.Spool.Filament)}  ·  {job.Printer.Name}";
            if (job.Result != PrintResult.Success)
            {
                subtitle = $"{DisplayText.ForResult(job.Result)}  ·  " + subtitle;
            }

            var row = new ListRow
            {
                Title = job.PrintModel.Name,
                Subtitle = subtitle,
                RightText = $"{cost:0.00}",
                RightSubText = $"{job.UsedGrams:0.#} g",
                DotColor = DarkTheme.GetResultColor(job.Result),
                DotGlow = true,
                ShowDeleteButton = true,
                Tag = job.Id
            };
            row.RowClicked += HistoryRow_Clicked;
            row.DeleteClicked += HistoryRow_DeleteClicked;
            list.Controls.Add(row);
        }

        summaryLabel.Text = $"{jobs.Count} baskı  ·  toplam maliyet {totalCost:0.00}";
        return list;
    }

    private void HistoryRow_Clicked(object? sender, EventArgs e)
    {
        if (sender is ListRow row)
        {
            ShowDialogAndRefresh(new PrintJobEditForm((int)row.Tag!));
        }
    }

    private void HistoryRow_DeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not ListRow row)
        {
            return;
        }

        bool delete = DarkMessageBox.AskYesNo(FindForm(),
            $"\"{row.Title}\" baskısı silinsin mi? Kullandığı gramlar makaraya geri döner.",
            "Baskıyı sil");
        if (!delete)
        {
            return;
        }

        using (var db = new AppDbContext())
        {
            PrintJob? job = db.PrintJobs.Find((int)row.Tag!);
            if (job != null)
            {
                db.PrintJobs.Remove(job);
                db.SaveChanges();
            }
        }
        ShowSelectedSection();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    // ----- Archived spools -----

    private RowListPanel BuildArchiveList()
    {
        List<Spool> spools;
        using (var db = new AppDbContext())
        {
            spools = db.Spools
                .Include(s => s.Filament)
                .Include(s => s.PrintJobs)
                .Where(s => s.IsArchived)
                .OrderByDescending(s => s.PurchaseDate)
                .ToList();
        }
        summaryLabel.Text = $"{spools.Count} arşivlenmiş makara";

        var list = new RowListPanel();
        if (spools.Count == 0)
        {
            list.ShowEmptyText("Arşivde makara yok.");
        }
        foreach (Spool spool in spools)
        {
            double usedGrams = spoolService.GetUsedGrams(spool.Id);
            var row = new ListRow
            {
                Title = $"#{spool.Id}  {DisplayText.ForFilament(spool.Filament)}",
                Subtitle = $"Alış: {spool.PurchaseDate.ToShortDateString()}  ·  {usedGrams:0} g kullanıldı  ·  {spool.PrintJobs.Count} baskı",
                RightText = "Arşivden çıkar",
                RightTextColor = DarkTheme.AccentColor,
                DotColor = HexColor.ToColor(spool.Filament.ColorHex),
                Tag = spool.Id
            };
            row.RowClicked += ArchiveRow_Clicked;
            list.Controls.Add(row);
        }
        return list;
    }

    private void ArchiveRow_Clicked(object? sender, EventArgs e)
    {
        if (sender is not ListRow row)
        {
            return;
        }

        if (DarkMessageBox.AskYesNo(FindForm(), "Bu makara tekrar makara rafına alınsın mı?", "Arşivden çıkar"))
        {
            spoolService.SetArchived((int)row.Tag!, false);
            ShowSelectedSection();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // ----- Add button and dialogs -----

    private void AddButton_Click(object? sender, EventArgs e)
    {
        switch (tabs.SelectedIndex)
        {
            case FilamentsTab:
                ShowDialogAndRefresh(new FilamentEditForm(null));
                break;
            case ModelsTab:
                ShowDialogAndRefresh(new ModelEditForm(null));
                break;
            case PrintersTab:
                ShowDialogAndRefresh(new PrinterEditForm(null));
                break;
            case HistoryTab:
                string? missingData = printJobService.FindMissingDataForNewJob();
                if (missingData != null)
                {
                    DarkMessageBox.ShowInfo(FindForm(), $"Önce {missingData} ekleyin.", "Baskı ekle");
                    return;
                }
                ShowDialogAndRefresh(new PrintJobEditForm(null));
                break;
        }
    }

    // Shows an add/edit dialog. If the user saved, the list and the main screen are refreshed.
    private void ShowDialogAndRefresh(Form dialog)
    {
        using (dialog)
        {
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }
        }
        ShowSelectedSection();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
