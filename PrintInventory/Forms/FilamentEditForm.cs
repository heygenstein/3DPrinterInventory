using PrintInventory.Data;

namespace PrintInventory.Forms;

// Dialog to add a new filament (filamentId = null) or edit an existing one.
public class FilamentEditForm : Form
{
    private readonly int? filamentId;
    private Color selectedColor = Color.Black;

    private readonly TextBox brandTextBox = new TextBox();
    private readonly ComboBox materialComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox colorNameTextBox = new TextBox();
    private readonly Panel colorSwatchPanel = new Panel
    {
        Size = new Size(28, 28),
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 0, 8, 0)
    };
    private readonly Label colorHexLabel = new Label
    {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 0, 8, 0)
    };
    private readonly NumericUpDown diameterInput = new DarkNumericUpDown
    {
        DecimalPlaces = 2,
        Increment = 0.05M,
        Minimum = 1M,
        Maximum = 3M,
        Value = 1.75M
    };
    private readonly NumericUpDown densityInput = new DarkNumericUpDown
    {
        DecimalPlaces = 2,
        Increment = 0.01M,
        Minimum = 0.5M,
        Maximum = 3M,
        Value = 1.24M
    };
    private readonly TextBox nozzleTempTextBox = new TextBox();
    private readonly TextBox bedTempTextBox = new TextBox();
    private readonly TextBox notesTextBox = UiTheme.CreateNotesTextBox();

    public FilamentEditForm(int? filamentId)
    {
        this.filamentId = filamentId;
        Text = filamentId == null ? "Filament Ekle" : "Filamenti Düzenle";
        UiTheme.ApplyDialogStyle(this);
        BuildLayout();

        if (filamentId == null)
        {
            ListItem.SelectById(materialComboBox, (int)FilamentMaterial.PLA);
            ShowSelectedColor();
        }
        else
        {
            LoadFilament(filamentId.Value);
        }
    }

    private void BuildLayout()
    {
        // Every material (PLA, PETG, ...) with its Turkish name; the Id is the enum value.
        DisplayText.FillMaterialComboBox(materialComboBox);

        Button chooseColorButton = new NeonButton("Seç...", isPrimary: false);
        chooseColorButton.Click += ChooseColorButton_Click;

        var colorPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0)
        };
        colorPanel.Controls.Add(colorSwatchPanel);
        colorPanel.Controls.Add(colorHexLabel);
        colorPanel.Controls.Add(chooseColorButton);

        TableLayoutPanel fieldsTable = UiTheme.CreateFieldsTable();
        UiTheme.AddFormRow(fieldsTable, "Marka", brandTextBox);
        UiTheme.AddFormRow(fieldsTable, "Malzeme", materialComboBox);
        UiTheme.AddFormRow(fieldsTable, "Renk adı", colorNameTextBox);
        UiTheme.AddFormRow(fieldsTable, "Renk", colorPanel);
        UiTheme.AddFormRow(fieldsTable, "Çap (mm)", diameterInput);
        UiTheme.AddFormRow(fieldsTable, "Yoğunluk (g/cm³)", densityInput);
        UiTheme.AddFormRow(fieldsTable, "Nozül sıcaklığı (°C)", nozzleTempTextBox);
        UiTheme.AddFormRow(fieldsTable, "Tabla sıcaklığı (°C)", bedTempTextBox);
        UiTheme.AddFormRow(fieldsTable, "Notlar", notesTextBox);

        Button saveButton = UiTheme.CreateSaveButton();
        saveButton.Click += SaveButton_Click;
        Button cancelButton = UiTheme.CreateCancelButton();

        UiTheme.BuildDialogLayout(this, fieldsTable, saveButton, cancelButton);
    }

    private void LoadFilament(int id)
    {
        using var db = new AppDbContext();
        Filament? filament = db.Filaments.Find(id);
        if (filament == null)
        {
            throw new InvalidOperationException($"Filament with Id {id} was not found.");
        }

        brandTextBox.Text = filament.Brand;
        ListItem.SelectById(materialComboBox, (int)filament.Material);
        colorNameTextBox.Text = filament.ColorName;
        selectedColor = HexColor.ToColor(filament.ColorHex);
        ShowSelectedColor();
        diameterInput.Value = (decimal)filament.DiameterMm;
        densityInput.Value = (decimal)filament.DensityGPerCm3;
        nozzleTempTextBox.Text = filament.RecommendedNozzleTempC?.ToString();
        bedTempTextBox.Text = filament.RecommendedBedTempC?.ToString();
        notesTextBox.Text = filament.Notes;
    }

    private void ShowSelectedColor()
    {
        colorSwatchPanel.BackColor = selectedColor;
        colorHexLabel.Text = HexColor.FromColor(selectedColor);
    }

    private void ChooseColorButton_Click(object? sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog
        {
            Color = selectedColor,
            FullOpen = true // show the custom color area too
        };

        if (colorDialog.ShowDialog(this) == DialogResult.OK)
        {
            selectedColor = colorDialog.Color;
            ShowSelectedColor();
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(brandTextBox.Text))
        {
            UiTheme.ShowValidationError(brandTextBox, "Marka zorunludur.");
            return;
        }
        if (string.IsNullOrWhiteSpace(colorNameTextBox.Text))
        {
            UiTheme.ShowValidationError(colorNameTextBox, "Renk adı zorunludur.");
            return;
        }
        if (!TryReadOptionalTemperature(nozzleTempTextBox, out int? nozzleTemp))
        {
            UiTheme.ShowValidationError(nozzleTempTextBox, "Nozül sıcaklığı 0 ile 500 arasında bir tam sayı olmalı ya da boş bırakılmalı.");
            return;
        }
        if (!TryReadOptionalTemperature(bedTempTextBox, out int? bedTemp))
        {
            UiTheme.ShowValidationError(bedTempTextBox, "Tabla sıcaklığı 0 ile 500 arasında bir tam sayı olmalı ya da boş bırakılmalı.");
            return;
        }

        using var db = new AppDbContext();
        Filament filament;
        if (filamentId == null)
        {
            filament = new Filament();
            db.Filaments.Add(filament);
        }
        else
        {
            filament = db.Filaments.Find(filamentId.Value)!;
        }

        filament.Brand = brandTextBox.Text.Trim();
        filament.Material = (FilamentMaterial)((ListItem)materialComboBox.SelectedItem!).Id;
        filament.ColorName = colorNameTextBox.Text.Trim();
        filament.ColorHex = HexColor.FromColor(selectedColor);
        filament.DiameterMm = (double)diameterInput.Value;
        filament.DensityGPerCm3 = (double)densityInput.Value;
        filament.RecommendedNozzleTempC = nozzleTemp;
        filament.RecommendedBedTempC = bedTemp;
        filament.Notes = notesTextBox.Text.Trim();

        db.SaveChanges();
        DialogResult = DialogResult.OK;
    }

    // Empty text = no value (null). Otherwise it must be a whole number from 0 to 500.
    // Returns false if the text is not valid.
    private static bool TryReadOptionalTemperature(TextBox textBox, out int? temperature)
    {
        temperature = null;
        string text = textBox.Text.Trim();
        if (text == "")
        {
            return true;
        }

        if (int.TryParse(text, out int value) && value >= 0 && value <= 500)
        {
            temperature = value;
            return true;
        }
        return false;
    }
}
