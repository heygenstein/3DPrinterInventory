namespace PrintInventory.Forms;

// Layout helpers for the add/edit dialogs (*EditForm, AdjustRemainingForm),
// so every dialog has the same dark look: labels on the left, inputs on the right,
// Kaydet / İptal at the bottom. Colors and fonts come from DarkTheme.
public static class UiTheme
{
    public const int InputWidth = 280;

    // Standard settings for an add/edit dialog. The dialog grows to fit its content.
    public static void ApplyDialogStyle(Form dialog)
    {
        dialog.Font = DarkTheme.NormalFont;
        dialog.BackColor = DarkTheme.CardColor;
        dialog.ForeColor = DarkTheme.TextColor;
        dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
        dialog.MaximizeBox = false;
        dialog.MinimizeBox = false;
        dialog.ShowInTaskbar = false;
        dialog.StartPosition = FormStartPosition.CenterParent;
        dialog.AutoSize = true;
        dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        dialog.Icon = DarkTheme.AppIcon;

        // The title bar can be made dark only after Windows created the window.
        dialog.HandleCreated += Dialog_HandleCreated;
    }

    private static void Dialog_HandleCreated(object? sender, EventArgs e)
    {
        if (sender is Form dialog)
        {
            DarkTheme.ApplyDarkTitleBar(dialog);
        }
    }

    // A two-column table: labels on the left, inputs on the right.
    public static TableLayoutPanel CreateFieldsTable()
    {
        var table = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 0
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        return table;
    }

    // Adds one "Label: [input]" row to a fields table.
    // labelAtTop: put the label at the top of the row (for tall inputs, e.g. a picture).
    public static void AddFormRow(TableLayoutPanel table, string labelText, Control input, bool labelAtTop = false)
    {
        bool isTallInput = labelAtTop || (input is TextBox textBox && textBox.Multiline);

        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            ForeColor = DarkTheme.MutedTextColor,
            Anchor = isTallInput ? AnchorStyles.Left | AnchorStyles.Top : AnchorStyles.Left,
            Margin = new Padding(0, 8, 16, 8)
        };

        input.Width = InputWidth;
        input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        input.Margin = new Padding(0, 5, 0, 5);
        StyleInput(input);

        int row = table.RowCount;
        table.RowCount = row + 1;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(label, 0, row);
        table.Controls.Add(input, 1, row);
    }

    // Dark colors for the standard input controls. For a container (for example a row with
    // a text box and a button) the controls inside it are styled too.
    public static void StyleInput(Control input)
    {
        // (Number boxes are DarkNumericUpDown and date pickers DarkDateTimePicker: they style themselves.)
        if (input is TextBox textBox)
        {
            textBox.BackColor = DarkTheme.InputColor;
            textBox.ForeColor = DarkTheme.TextColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            if (textBox.Multiline)
            {
                DarkTheme.ApplyDarkScrollBars(textBox);
            }
        }
        else if (input is ComboBox comboBox)
        {
            comboBox.BackColor = DarkTheme.InputColor;
            comboBox.ForeColor = DarkTheme.TextColor;
            DarkTheme.ApplyDarkComboBox(comboBox);
        }
        else if (input is FlowLayoutPanel || input is TableLayoutPanel)
        {
            foreach (Control child in input.Controls)
            {
                StyleInput(child);
            }
        }
    }

    public static Button CreateSaveButton()
    {
        return new NeonButton("Kaydet", isPrimary: true);
    }

    public static Button CreateCancelButton()
    {
        return new NeonButton("İptal", isPrimary: false)
        {
            DialogResult = DialogResult.Cancel // closes the dialog automatically
        };
    }

    // Puts the fields table and the Save/Cancel buttons together and adds them to the dialog.
    public static void BuildDialogLayout(Form dialog, TableLayoutPanel fieldsTable, Button saveButton, Button cancelButton)
    {
        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 18, 0, 0)
        };
        cancelButton.Margin = new Padding(0);
        saveButton.Margin = new Padding(0, 0, 10, 0);
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(saveButton);

        var mainTable = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(24, 20, 24, 20)
        };
        mainTable.Controls.Add(fieldsTable, 0, 0);
        mainTable.Controls.Add(buttonsPanel, 0, 1);

        dialog.Controls.Add(mainTable);
        dialog.AcceptButton = saveButton;   // Enter key = Save
        dialog.CancelButton = cancelButton; // Esc key = Cancel
    }

    public static TextBox CreateNotesTextBox()
    {
        return new TextBox
        {
            Multiline = true,
            Height = 80,
            ScrollBars = ScrollBars.Vertical
        };
    }

    public static void ShowValidationError(Control input, string message)
    {
        DarkMessageBox.ShowWarning(input.FindForm(), message, "Girdiyi kontrol edin");
        input.Focus();
    }
}
