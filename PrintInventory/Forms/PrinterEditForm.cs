using PrintInventory.Data;

namespace PrintInventory.Forms;

// Dialog to add a new printer (printerId = null) or edit an existing one.
public class PrinterEditForm : Form
{
    private readonly int? printerId;

    private readonly TextBox nameTextBox = new TextBox();
    private readonly TextBox brandTextBox = new TextBox();
    private readonly TextBox modelTextBox = new TextBox();
    private readonly NumericUpDown nozzleInput = new DarkNumericUpDown
    {
        DecimalPlaces = 2,
        Increment = 0.1M,
        Minimum = 0.1M,
        Maximum = 2M,
        Value = 0.4M
    };
    private readonly NumericUpDown buildXInput = CreateMillimeterInput(220);
    private readonly NumericUpDown buildYInput = CreateMillimeterInput(220);
    private readonly NumericUpDown buildZInput = CreateMillimeterInput(250);
    private readonly TextBox notesTextBox = UiTheme.CreateNotesTextBox();

    public PrinterEditForm(int? printerId)
    {
        this.printerId = printerId;
        Text = printerId == null ? "Yazıcı Ekle" : "Yazıcıyı Düzenle";
        UiTheme.ApplyDialogStyle(this);
        BuildLayout();

        if (printerId != null)
        {
            LoadPrinter(printerId.Value);
        }
    }

    private static NumericUpDown CreateMillimeterInput(int defaultValue)
    {
        return new DarkNumericUpDown { Minimum = 1, Maximum = 2000, Value = defaultValue };
    }

    private void BuildLayout()
    {
        TableLayoutPanel fieldsTable = UiTheme.CreateFieldsTable();
        UiTheme.AddFormRow(fieldsTable, "Ad", nameTextBox);
        UiTheme.AddFormRow(fieldsTable, "Marka", brandTextBox);
        UiTheme.AddFormRow(fieldsTable, "Model", modelTextBox);
        UiTheme.AddFormRow(fieldsTable, "Nozül çapı (mm)", nozzleInput);
        UiTheme.AddFormRow(fieldsTable, "Baskı alanı X (mm)", buildXInput);
        UiTheme.AddFormRow(fieldsTable, "Baskı alanı Y (mm)", buildYInput);
        UiTheme.AddFormRow(fieldsTable, "Baskı alanı Z (mm)", buildZInput);
        UiTheme.AddFormRow(fieldsTable, "Notlar", notesTextBox);

        Button saveButton = UiTheme.CreateSaveButton();
        saveButton.Click += SaveButton_Click;
        Button cancelButton = UiTheme.CreateCancelButton();

        UiTheme.BuildDialogLayout(this, fieldsTable, saveButton, cancelButton);
    }

    private void LoadPrinter(int id)
    {
        using var db = new AppDbContext();
        Printer? printer = db.Printers.Find(id);
        if (printer == null)
        {
            throw new InvalidOperationException($"Printer with Id {id} was not found.");
        }

        nameTextBox.Text = printer.Name;
        brandTextBox.Text = printer.Brand;
        modelTextBox.Text = printer.Model;
        nozzleInput.Value = (decimal)printer.NozzleDiameterMm;
        buildXInput.Value = printer.BuildVolumeXMm;
        buildYInput.Value = printer.BuildVolumeYMm;
        buildZInput.Value = printer.BuildVolumeZMm;
        notesTextBox.Text = printer.Notes;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            UiTheme.ShowValidationError(nameTextBox, "Ad zorunludur.");
            return; // keep the dialog open
        }

        using var db = new AppDbContext();
        Printer printer;
        if (printerId == null)
        {
            printer = new Printer();
            db.Printers.Add(printer);
        }
        else
        {
            // "!" tells the compiler "this is not null": we loaded this printer when the dialog opened.
            printer = db.Printers.Find(printerId.Value)!;
        }

        printer.Name = nameTextBox.Text.Trim();
        printer.Brand = brandTextBox.Text.Trim();
        printer.Model = modelTextBox.Text.Trim();
        printer.NozzleDiameterMm = (double)nozzleInput.Value;
        printer.BuildVolumeXMm = (int)buildXInput.Value;
        printer.BuildVolumeYMm = (int)buildYInput.Value;
        printer.BuildVolumeZMm = (int)buildZInput.Value;
        printer.Notes = notesTextBox.Text.Trim();

        db.SaveChanges();
        DialogResult = DialogResult.OK; // closes the dialog and tells the caller we saved
    }
}
