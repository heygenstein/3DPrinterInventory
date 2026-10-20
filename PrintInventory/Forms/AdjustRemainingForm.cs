using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// Dialog where the user enters the real (weighed) remaining grams of a spool.
// SpoolService then calculates the new AdjustmentGrams.
public class AdjustRemainingForm : Form
{
    private readonly int spoolId;
    private readonly SpoolService spoolService = new SpoolService();

    private readonly Label filamentLabel = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label calculatedLabel = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly NumericUpDown actualRemainingInput = new DarkNumericUpDown
    {
        DecimalPlaces = 1,
        Minimum = 0,
        Maximum = 10000,
        Increment = 10
    };

    public AdjustRemainingForm(int spoolId)
    {
        this.spoolId = spoolId;
        Text = "Kalan Miktarı Düzelt";
        UiTheme.ApplyDialogStyle(this);
        BuildLayout();
        LoadSpool();
    }

    private void BuildLayout()
    {
        var hintLabel = new Label
        {
            Text = "Makarayı tartın ve boş makaranın ağırlığını çıkarın.",
            AutoSize = true,
            ForeColor = DarkTheme.MutedTextColor,
            Anchor = AnchorStyles.Left
        };

        TableLayoutPanel fieldsTable = UiTheme.CreateFieldsTable();
        UiTheme.AddFormRow(fieldsTable, "Makara", filamentLabel);
        UiTheme.AddFormRow(fieldsTable, "Hesaplanan kalan", calculatedLabel);
        UiTheme.AddFormRow(fieldsTable, "Gerçek kalan (g)", actualRemainingInput);
        UiTheme.AddFormRow(fieldsTable, "", hintLabel);

        Button saveButton = UiTheme.CreateSaveButton();
        saveButton.Click += SaveButton_Click;
        Button cancelButton = UiTheme.CreateCancelButton();

        UiTheme.BuildDialogLayout(this, fieldsTable, saveButton, cancelButton);
    }

    private void LoadSpool()
    {
        Spool? spool;
        using (var db = new AppDbContext())
        {
            spool = db.Spools.Include(s => s.Filament).FirstOrDefault(s => s.Id == spoolId);
        }
        if (spool == null)
        {
            throw new InvalidOperationException($"Spool with Id {spoolId} was not found.");
        }

        double remainingGrams = spoolService.GetRemainingGrams(spoolId);
        filamentLabel.Text = $"#{spool.Id} {DisplayText.ForFilament(spool.Filament)}";
        calculatedLabel.Text = $"{remainingGrams:0.0} g";

        // Start with the calculated value, so the user only changes the difference.
        // A NumericUpDown cannot show a value below its Minimum (0), so clamp it.
        actualRemainingInput.Value = (decimal)Math.Max(0, remainingGrams);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        spoolService.AdjustRemaining(spoolId, (double)actualRemainingInput.Value);
        DialogResult = DialogResult.OK;
    }
}
