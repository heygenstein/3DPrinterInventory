using PrintInventory.Data;

namespace PrintInventory.Forms;

// Dialog to add a new spool (spoolId = null) or edit an existing one.
// Remaining grams is not edited here: use "Adjust remaining" for that.
public class SpoolEditForm : Form
{
    private readonly int? spoolId;

    private readonly ComboBox filamentComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown initialWeightInput = new DarkNumericUpDown
    {
        Minimum = 1,
        Maximum = 10000,
        Increment = 50,
        Value = 1000
    };
    private readonly NumericUpDown pricePaidInput = new DarkNumericUpDown
    {
        DecimalPlaces = 2,
        Minimum = 0,
        Maximum = 100000
    };
    private readonly DarkDateTimePicker purchaseDatePicker = new DarkDateTimePicker
    {
        Value = DateTime.Today
    };
    private readonly NumericUpDown lowStockThresholdInput = new DarkNumericUpDown
    {
        Minimum = 0,
        Maximum = 10000,
        Increment = 10,
        Value = 150
    };
    private readonly TextBox notesTextBox = UiTheme.CreateNotesTextBox();

    public SpoolEditForm(int? spoolId)
    {
        this.spoolId = spoolId;
        Text = spoolId == null ? "Makara Ekle" : "Makarayı Düzenle";
        UiTheme.ApplyDialogStyle(this);
        BuildLayout();
        LoadFilaments();

        if (spoolId == null)
        {
            if (filamentComboBox.Items.Count > 0)
            {
                filamentComboBox.SelectedIndex = 0;
            }
        }
        else
        {
            LoadSpool(spoolId.Value);
        }
    }

    private void BuildLayout()
    {
        TableLayoutPanel fieldsTable = UiTheme.CreateFieldsTable();
        UiTheme.AddFormRow(fieldsTable, "Filament", filamentComboBox);
        UiTheme.AddFormRow(fieldsTable, "Başlangıç ağırlığı (g)", initialWeightInput);
        UiTheme.AddFormRow(fieldsTable, "Ödenen fiyat", pricePaidInput);
        UiTheme.AddFormRow(fieldsTable, "Alış tarihi", purchaseDatePicker);
        UiTheme.AddFormRow(fieldsTable, "Az stok uyarısı (g)", lowStockThresholdInput);
        UiTheme.AddFormRow(fieldsTable, "Notlar", notesTextBox);

        Button saveButton = UiTheme.CreateSaveButton();
        saveButton.Click += SaveButton_Click;
        Button cancelButton = UiTheme.CreateCancelButton();

        UiTheme.BuildDialogLayout(this, fieldsTable, saveButton, cancelButton);
    }

    private void LoadFilaments()
    {
        using var db = new AppDbContext();
        List<Filament> filaments = db.Filaments
            .OrderBy(f => f.Brand.ToLower())
            .ThenBy(f => f.ColorName.ToLower())
            .ToList();

        foreach (Filament filament in filaments)
        {
            filamentComboBox.Items.Add(new ListItem(filament.Id, DisplayText.ForFilament(filament)));
        }
    }

    private void LoadSpool(int id)
    {
        using var db = new AppDbContext();
        Spool? spool = db.Spools.Find(id);
        if (spool == null)
        {
            throw new InvalidOperationException($"Spool with Id {id} was not found.");
        }

        ListItem.SelectById(filamentComboBox, spool.FilamentId);
        initialWeightInput.Value = (decimal)spool.InitialWeightGrams;
        pricePaidInput.Value = spool.PricePaid;
        purchaseDatePicker.Value = spool.PurchaseDate;
        lowStockThresholdInput.Value = (decimal)spool.LowStockThresholdGrams;
        notesTextBox.Text = spool.Notes;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (filamentComboBox.SelectedItem is not ListItem selectedFilament)
        {
            UiTheme.ShowValidationError(filamentComboBox, "Lütfen bir filament seçin.");
            return;
        }

        using var db = new AppDbContext();
        Spool spool;
        if (spoolId == null)
        {
            spool = new Spool();
            db.Spools.Add(spool);
        }
        else
        {
            spool = db.Spools.Find(spoolId.Value)!;
        }

        spool.FilamentId = selectedFilament.Id;
        spool.InitialWeightGrams = (double)initialWeightInput.Value;
        spool.PricePaid = pricePaidInput.Value;
        spool.PurchaseDate = purchaseDatePicker.Value.Date;
        spool.LowStockThresholdGrams = (double)lowStockThresholdInput.Value;
        spool.Notes = notesTextBox.Text.Trim();

        db.SaveChanges();
        DialogResult = DialogResult.OK;
    }
}
