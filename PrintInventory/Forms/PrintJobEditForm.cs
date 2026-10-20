using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// Dialog to add a new print job (jobId = null) or edit an existing one.
// Shows the cost and the grams left on the spool while the user types.
public class PrintJobEditForm : Form
{
    private readonly int? jobId;
    private readonly PrintJobService printJobService = new PrintJobService();

    private readonly ComboBox modelComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox printerComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox spoolComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DarkDateTimePicker printDatePicker = new DarkDateTimePicker
    {
        CustomFormat = "dd.MM.yyyy  HH:mm"
    };
    private readonly NumericUpDown usedGramsInput = new DarkNumericUpDown
    {
        DecimalPlaces = 1,
        Minimum = 0,
        Maximum = 10000
    };
    private readonly ComboBox resultComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label spoolInfoLabel = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly TextBox notesTextBox = UiTheme.CreateNotesTextBox();

    public PrintJobEditForm(int? jobId)
    {
        this.jobId = jobId;
        Text = jobId == null ? "Baskı Ekle" : "Baskıyı Düzenle";
        UiTheme.ApplyDialogStyle(this);
        BuildLayout();

        PrintJob? job = null;
        if (jobId != null)
        {
            job = LoadJob(jobId.Value);
        }

        // An edited job may use a spool that is archived now; that spool must still be in the list.
        LoadChoices(job?.SpoolId);

        if (job == null)
        {
            SetDefaultsForNewJob();
        }
        else
        {
            ShowJob(job);
        }

        // Connect the events only now. Otherwise filling the inputs above would already
        // run them, and (for example) overwrite the saved UsedGrams with the model estimate.
        modelComboBox.SelectedIndexChanged += ModelComboBox_SelectedIndexChanged;
        spoolComboBox.SelectedIndexChanged += SpoolOrGrams_Changed;
        usedGramsInput.ValueChanged += SpoolOrGrams_Changed;
        UpdateSpoolInfo();
    }

    private void BuildLayout()
    {
        // Success, Failed, Cancelled with their Turkish names; the Id is the enum value.
        DisplayText.FillResultComboBox(resultComboBox);

        TableLayoutPanel fieldsTable = UiTheme.CreateFieldsTable();
        UiTheme.AddFormRow(fieldsTable, "Model", modelComboBox);
        UiTheme.AddFormRow(fieldsTable, "Yazıcı", printerComboBox);
        UiTheme.AddFormRow(fieldsTable, "Makara", spoolComboBox);
        UiTheme.AddFormRow(fieldsTable, "Tarih", printDatePicker);
        UiTheme.AddFormRow(fieldsTable, "Kullanılan gram", usedGramsInput);
        UiTheme.AddFormRow(fieldsTable, "", spoolInfoLabel);
        UiTheme.AddFormRow(fieldsTable, "Sonuç", resultComboBox);
        UiTheme.AddFormRow(fieldsTable, "Notlar", notesTextBox);

        Button saveButton = UiTheme.CreateSaveButton();
        saveButton.Click += SaveButton_Click;
        Button cancelButton = UiTheme.CreateCancelButton();

        UiTheme.BuildDialogLayout(this, fieldsTable, saveButton, cancelButton);
    }

    private static PrintJob LoadJob(int id)
    {
        using var db = new AppDbContext();
        PrintJob? job = db.PrintJobs.Find(id);
        if (job == null)
        {
            throw new InvalidOperationException($"Print job with Id {id} was not found.");
        }
        return job;
    }

    private void LoadChoices(int? spoolIdToInclude)
    {
        using var db = new AppDbContext();

        foreach (PrintModel model in db.PrintModels.OrderBy(m => m.Name.ToLower()))
        {
            modelComboBox.Items.Add(new ListItem(model.Id, model.Name));
        }

        foreach (Printer printer in db.Printers.OrderBy(p => p.Name.ToLower()))
        {
            printerComboBox.Items.Add(new ListItem(printer.Id, printer.Name));
        }

        List<Spool> spools = db.Spools
            .Include(s => s.Filament)
            .Where(s => !s.IsArchived || s.Id == spoolIdToInclude)
            .OrderBy(s => s.Filament.Brand.ToLower())
            .ThenBy(s => s.Filament.ColorName.ToLower())
            .ThenBy(s => s.Id)
            .ToList();
        foreach (Spool spool in spools)
        {
            string text = $"#{spool.Id} {DisplayText.ForFilament(spool.Filament)}";
            if (spool.IsArchived)
            {
                text += " (arşivde)";
            }
            spoolComboBox.Items.Add(new ListItem(spool.Id, text));
        }
    }

    private void SetDefaultsForNewJob()
    {
        SelectFirstItem(modelComboBox);
        SelectFirstItem(printerComboBox);
        SelectFirstItem(spoolComboBox);
        printDatePicker.Value = DateTime.Now;
        ListItem.SelectById(resultComboBox, (int)PrintResult.Success);
        SetUsedGramsFromModelEstimate();
    }

    private static void SelectFirstItem(ComboBox comboBox)
    {
        if (comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private void ShowJob(PrintJob job)
    {
        ListItem.SelectById(modelComboBox, job.PrintModelId);
        ListItem.SelectById(printerComboBox, job.PrinterId);
        ListItem.SelectById(spoolComboBox, job.SpoolId);
        printDatePicker.Value = job.PrintDate;
        usedGramsInput.Value = (decimal)job.UsedGrams;
        ListItem.SelectById(resultComboBox, (int)job.Result);
        notesTextBox.Text = job.Notes;
    }

    // Rule: UsedGrams starts as the model's EstimatedGrams. The user can change it
    // (for example a failed print uses less).
    private void SetUsedGramsFromModelEstimate()
    {
        if (modelComboBox.SelectedItem is not ListItem selectedModel)
        {
            return;
        }

        using var db = new AppDbContext();
        PrintModel? model = db.PrintModels.Find(selectedModel.Id);
        if (model != null)
        {
            usedGramsInput.Value = Math.Min((decimal)model.EstimatedGrams, usedGramsInput.Maximum);
        }
    }

    private void ModelComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        SetUsedGramsFromModelEstimate();
    }

    private void SpoolOrGrams_Changed(object? sender, EventArgs e)
    {
        UpdateSpoolInfo();
    }

    // Shows "Available on spool: 650 g   Cost: 6.75" under the grams input.
    // Turns red when the job uses more than the spool has left.
    private void UpdateSpoolInfo()
    {
        if (spoolComboBox.SelectedItem is not ListItem selectedSpool)
        {
            spoolInfoLabel.Text = "";
            return;
        }

        Spool? spool;
        using (var db = new AppDbContext())
        {
            spool = db.Spools.Find(selectedSpool.Id);
        }
        if (spool == null)
        {
            spoolInfoLabel.Text = "";
            return;
        }

        double usedGrams = (double)usedGramsInput.Value;
        double availableGrams = printJobService.GetAvailableGrams(spool.Id, jobId);
        decimal cost = printJobService.CalculateCost(spool, usedGrams);

        string text = $"Makarada kalan: {availableGrams:0} g     Maliyet: {cost:0.00}";
        if (printJobService.UsesMoreThanAvailable(usedGrams, availableGrams))
        {
            spoolInfoLabel.Text = text + "   (kalandan fazla!)";
            spoolInfoLabel.ForeColor = DarkTheme.WarningColor;
        }
        else
        {
            spoolInfoLabel.Text = text;
            spoolInfoLabel.ForeColor = DarkTheme.MutedTextColor;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (modelComboBox.SelectedItem is not ListItem selectedModel)
        {
            UiTheme.ShowValidationError(modelComboBox, "Lütfen bir model seçin.");
            return;
        }
        if (printerComboBox.SelectedItem is not ListItem selectedPrinter)
        {
            UiTheme.ShowValidationError(printerComboBox, "Lütfen bir yazıcı seçin.");
            return;
        }
        if (spoolComboBox.SelectedItem is not ListItem selectedSpool)
        {
            UiTheme.ShowValidationError(spoolComboBox, "Lütfen bir makara seçin.");
            return;
        }

        // Rule: warn, but let the user decide. The calculated remaining value may be wrong
        // (for example the spool was never adjusted), so we must not block the save.
        double usedGrams = (double)usedGramsInput.Value;
        double availableGrams = printJobService.GetAvailableGrams(selectedSpool.Id, jobId);
        if (printJobService.UsesMoreThanAvailable(usedGrams, availableGrams))
        {
            bool saveAnyway = DarkMessageBox.AskYesNo(this,
                $"Bu baskı {usedGrams:0.#} g kullanıyor, ama makarada sadece {availableGrams:0.#} g kaldı.\n\n" +
                "Yine de kaydedilsin mi?",
                "Yetersiz filament");
            if (!saveAnyway)
            {
                return;
            }
        }

        using var db = new AppDbContext();
        PrintJob job;
        if (jobId == null)
        {
            job = new PrintJob();
            db.PrintJobs.Add(job);
        }
        else
        {
            job = db.PrintJobs.Find(jobId.Value)!;
        }

        job.PrintModelId = selectedModel.Id;
        job.PrinterId = selectedPrinter.Id;
        job.SpoolId = selectedSpool.Id;
        job.PrintDate = printDatePicker.Value;
        job.UsedGrams = usedGrams;
        job.Result = (PrintResult)((ListItem)resultComboBox.SelectedItem!).Id;
        job.Notes = notesTextBox.Text.Trim();

        db.SaveChanges();
        DialogResult = DialogResult.OK;
    }
}
